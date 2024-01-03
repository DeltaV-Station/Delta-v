#!/usr/bin/python
# Tidy a map of any unnecessary datafields.

import argparse
import locale
from datetime import datetime
from pathlib import Path
from ruamel import yaml
from sys import argv


def capitalized_bool_dumper(representer, data):
    tag = "tag:yaml.org,2002:bool"
    value = "True" if data else "False"

    return representer.represent_scalar(tag, value)


# These components should be okay to remove entirely.
REMOVE_COMPONENTS = [
    "AmbientSound",
    "EmitSoundOnCollide",
    "Fixtures",
    "GravityShake",
    "HandheldLight", # Floodlights are serializing these?
    "PlaySoundBehaviour",
]

# The component will have these fields removed, and if there is no other data
# left, the component itself will be removed.
REMOVE_COMPONENT_DATA = {
    "Airtight": ["airBlocked"],
    "DeepFryer": ["nextFryTime"],
    "Defibrillator": ["nextZapTime"],
    "Door": ["state", "SecondsUntilStateChange"],
    "Gun": ["nextFire"],
    "MaterialReclaimer": ["nextSound"],
    "MeleeWeapon": ["nextAttack"],
    "Occluder": ["enabled"],
    "Physics": ["canCollide"],
    "PowerCellDraw": ["nextUpdate"],
    "SolutionPurge": ["nextPurgeTime"],
    "SolutionRegeneration": ["nextChargeTime"],
    "SuitSensor": ["nextUpdate"],
    "Thruster": ["nextFire"],
    "VendingMachine": ["nextEmpEject"],
}

# Remove only these fields from the components.
# The component will be kept no matter what.
ERASE_COMPONENT_DATA = {
    "GridPathfinding": ["nextUpdate"],
    "SpreaderGrid": ["nextUpdate"],
}


def tidy_entity(entity):
    components = entity["components"]

    for i in range(len(components) - 1, 0, -1):
        component = components[i]
        ctype = component["type"]

        # Remove unnecessary components.
        if ctype in REMOVE_COMPONENTS:
            del components[i]

        # Remove unnecessary datafields and empty components.
        elif ctype in REMOVE_COMPONENT_DATA:
            datafields_to_remove = REMOVE_COMPONENT_DATA[ctype]

            for datafield in datafields_to_remove:
                try:
                    del component[datafield]
                except KeyError:
                    pass

            # The only field left has to be the type, so remove the component entirely.
            if len(component.keys()) == 1:
                del components[i]

        # Remove unnecessary datafields only.
        elif ctype in ERASE_COMPONENT_DATA:
            datafields_to_remove = ERASE_COMPONENT_DATA[ctype]

            for datafield in datafields_to_remove:
                try:
                    del component[datafield]
                except KeyError:
                    pass

def tidy_map(map_data):
    # Iterate through all of the map's prototypes.
    for map_prototype in map_data["entities"]:

        # Iterate through all of the instances of said prototype.
        for map_entity in map_prototype["entities"]:
            tidy_entity(map_entity)


def main():
    locale.setlocale(locale.LC_ALL, '')

    parser = argparse.ArgumentParser(description='Tidy a map of any unnecessary datafields')

    parser.add_argument('--infile', type=str,
                        required=True,
                        help='which map file to load')

    parser.add_argument('--outfile', type=str,
                        help='where to save the cleaned map to')

    args = parser.parse_args()

    # SS14 saves some booleans as "True" and others as "true", so.
    # If it's ever necessary that we use some specific format, re-enable this.
    # yaml.RoundTripRepresenter.add_representer(bool, capitalized_bool_dumper)

    # Load the map.
    infname = args.infile
    print(f"Loading {infname} ...")
    load_time = datetime.now()
    infile = open(infname, 'r')
    map_data = yaml.load(infile, Loader=yaml.RoundTripLoader)
    infile.close()
    print(f"Loaded in {datetime.now() - load_time}\n")

    # Clean it.
    print(f"Cleaning map ...")
    clean_time = datetime.now()
    tidy_map(map_data)
    print(f"Cleaned in {datetime.now() - clean_time}\n")

    # Save it.
    outfname = args.outfile

    if outfname == None:
        # No output filename was specified, so add a suffix to the input filename.
        outfname = Path(args.infile)
        outfname = outfname.with_stem(outfname.stem + "_tidy")

    # Force *nix line-endings.
    # It's one less byte per line and maps are heavy on lines.
    newline = '\n'

    print(f"Saving cleaned map to {outfname} ...")
    save_time = datetime.now()
    outfile = open(outfname, 'w', newline=newline)
    yaml.boolean_representation = ['False', 'True']
    serialized = yaml.dump(map_data, Dumper=yaml.RoundTripDumper) + "...\n"
    outfile.write(serialized)
    outfile.close()
    print(f"Saved in {datetime.now() - save_time}\n")

    print("Done!")

    start_size = Path(infname).stat().st_size
    end_size = Path(outfname).stat().st_size
    print(f"Saved {start_size - end_size:n} bytes.")

if __name__ == "__main__":
    main()

