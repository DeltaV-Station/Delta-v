#!/usr/bin/env python3
"""
Space Station 14 Map Tidying Tool
Original work Copyright (c) 2023 Magil
Modified work Copyright (c) 2024 DeltaV-Station

This script is licensed under MIT
Modifications include code modernization, restructuring, and YAML handling updates
"""

import argparse
import locale
from datetime import datetime
from pathlib import Path
from typing import Dict, List, Any
from ruamel.yaml import YAML


# Configuration for components that should be handled during tidying
class TidyConfig:
    # Components that should be removed entirely
    REMOVE_COMPONENTS: List[str] = [
        "AmbientSound",
        "EmitSoundOnCollide",
        "Fixtures",
        "GravityShake",
        "HandheldLight",  # Floodlights are serializing these?
        "PlaySoundBehaviour",
    ]

    # Components that will have specific fields removed and may be removed entirely if empty
    REMOVE_COMPONENT_DATA: Dict[str, List[str]] = {
        "Airtight": ["airBlocked"],
        "DeepFryer": ["nextFryTime"],
        "Door": ["state", "secondsUntilStateChange"],
        "MaterialReclaimer": ["nextSound"],
        "Occluder": ["enabled"],
        "Physics": ["canCollide"],
    }

    # Fields to remove from components while keeping the component itself
    ERASE_COMPONENT_DATA: Dict[str, List[str]] = {
        "GridPathfinding": ["nextUpdate"],
    }


class MapTidier:
    def __init__(self):
        self.yaml = YAML()
        self.yaml.preserve_quotes = True
        self.yaml.width = 4096  # Prevent line wrapping
        # Set indentation to match the weird format
        self.yaml.indent(mapping=2, sequence=2, offset=0)

    @staticmethod
    def tidy_entity(entity: Dict[str, Any]) -> None:
        """
        Clean up unnecessary data from a single entity.
        """
        if "components" not in entity:
            return

        components = entity["components"]
        if not isinstance(components, list):
            return

        # Iterate backwards to safely remove items
        for i in range(len(components) - 1, -1, -1):
            if i >= len(components):  # Safety check in case of removals
                continue

            component = components[i]
            if not isinstance(component, dict) or "type" not in component:
                continue

            ctype = component["type"]

            # Handle complete component removal
            if ctype in TidyConfig.REMOVE_COMPONENTS:
                del components[i]
                continue

            # Handle component data removal with possible complete removal
            if ctype in TidyConfig.REMOVE_COMPONENT_DATA:
                datafields = TidyConfig.REMOVE_COMPONENT_DATA[ctype]
                for field in datafields:
                    component.pop(field, None)

                # Remove component if only type remains
                if len(component) == 1:  # Only 'type' field remains
                    del components[i]
                continue

            # Handle selective data removal
            if ctype in TidyConfig.ERASE_COMPONENT_DATA:
                datafields = TidyConfig.ERASE_COMPONENT_DATA[ctype]
                for field in datafields:
                    component.pop(field, None)

    def tidy_map(self, map_data: Dict[str, Any]) -> None:
        """
        Process and clean the entire map data structure.
        """
        if "entities" not in map_data:
            return

        for prototype in map_data["entities"]:
            if "entities" not in prototype:
                continue

            for entity in prototype["entities"]:
                self.tidy_entity(entity)


class MapProcessor:
    def __init__(self, infile: str, outfile: str | None = None):
        self.infile = Path(infile)
        self.outfile = Path(outfile) if outfile else self.infile.with_stem(f"{self.infile.stem}_tidy")
        self.tidier = MapTidier()

    def process(self) -> None:
        """
        Load, process, and save the map file.
        """
        # Load
        print(f"Loading {self.infile} ...")
        load_time = datetime.now()
        map_data = self._load_map()
        print(f"Loaded in {datetime.now() - load_time}\n")

        # Clean
        print("Cleaning map ...")
        clean_time = datetime.now()
        self.tidier.tidy_map(map_data)
        print(f"Cleaned in {datetime.now() - clean_time}\n")

        # Save
        print(f"Saving cleaned map to {self.outfile} ...")
        save_time = datetime.now()
        self._save_map(map_data)
        print(f"Saved in {datetime.now() - save_time}\n")

        # Report size difference
        self._report_size_difference()

    def _load_map(self) -> Dict[str, Any]:
        """Load and parse the YAML map file."""
        with open(self.infile, 'r') as f:
            return self.tidier.yaml.load(f)

    def _save_map(self, map_data: Dict[str, Any]) -> None:
        """Save the processed map data to file."""
        with open(self.outfile, 'w', newline='\n') as f:
            self.tidier.yaml.dump(map_data, f)
            f.write("...\n")  # Add YAML document end marker

    def _report_size_difference(self) -> None:
        """Calculate and report the size difference between input and output files."""
        start_size = self.infile.stat().st_size
        end_size = self.outfile.stat().st_size
        saved_bytes = start_size - end_size
        print(f"Saved {saved_bytes:n} bytes ({saved_bytes / start_size:.1%} reduction)")


def main():
    locale.setlocale(locale.LC_ALL, '')

    parser = argparse.ArgumentParser(
        description='Tidy Space Station 14 map files by removing unnecessary data fields'
    )
    parser.add_argument(
        '--infile',
        type=str,
        required=True,
        help='input map file to process'
    )
    parser.add_argument(
        '--outfile',
        type=str,
        help='output file for the cleaned map (defaults to input_tidy)'
    )

    args = parser.parse_args()

    try:
        processor = MapProcessor(args.infile, args.outfile)
        processor.process()
        print("Done!")
    except Exception as e:
        print(f"Error processing map: {e}")
        raise


if __name__ == "__main__":
    main()
