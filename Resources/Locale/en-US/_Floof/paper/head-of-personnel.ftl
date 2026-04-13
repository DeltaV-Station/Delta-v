paperwork-header-hop = {"["}color=#1b67a5] ◥[bold]N[/bold]◣ [head=3][bold]HEAD OF PERSONNEL[/bold][/head][/color]
paperwork-separator-hop = {"["}color=#1b67a5]‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾[/color]

paperwork-hop-access-permit =
    { paperwork-header-hop }
    { paperwork-separator-hop }
        {"["}head=2]Access Permit [/head]
    { paperwork-separator-hop }
        {"["}bold]SECTOR:[/bold] Floof Station
        {"["}bold]STATION:[/bold] {$stationName}
        {"["}bold]SHIFT TIME:[/bold] XX:XX
        {"["}bold]PERSONS:[/bold]
        {"["}bold]ACCESS GRANTED:[/bold]
        {"["}bold]JOB:[/bold]
    { paperwork-separator-hop }
    {"•"} Reason For Extended Access:
    {"•"} Other Notes:
    {"["}color=red]This document gives {"{"}insert name{"}"} the ability to Access stated areas unhindered.

    This document must be signed, and stamped by the Head of Personnel to be considered valid.[/color]

    {"["}bold]Applicant's Signature:[/bold]
    {"["}bold]Head of Personnel's Signature:[/bold]

paperwork-hop-job-transfer-request =
    { paperwork-header-hop }
    { paperwork-separator-hop }
        {"["}head=2]Job Transfer Request[/head]
    { paperwork-separator-hop }
        {"["}bold]SECTOR:[/bold] Floof Station
        {"["}bold]STATION:[/bold] {$stationName}
        {"["}bold]SHIFT TIME:[/bold] XX:XX
        {"["}bold]APPLICANT:[/bold]
        {"["}bold]JOB:[/bold]
    { paperwork-separator-hop }
    {"•"} Desired position:
    {"•"} Reasons for transfer to the desired position:
    {"•"} Your skills:
    { paperwork-separator-hop }
    {"["}color=red]This document is not valid unless stamped by at least one of the required personnel.[/color]

    {"["}italic]NOTE: The document is valid if a seal of the Head of the department where the accountable person is employed, unless explicitly stated otherwise. The accountable person confirms that they have all the necessary skills to work in a new workplace.[/italic]

    {"["}bold]Applicants's Signature:[/bold]
    {"["}bold]Head Of Personnel's Signature:[/bold]


paperwork-hop-crewmate-identification =
    { paperwork-header-hop }
    { paperwork-separator-hop }
        {"["}head=2]CREWMATE IDENTIFICATION[/head]
    { paperwork-separator-hop }
        {"["}bold]SECTOR:[/bold] Floof Station
        {"["}bold]NAME:[/bold]
        {"["}bold]PLACE OF ORIGIN:[/bold]
        {"["}bold]DATE OF BIRTH:[/bold]
        {"["}bold]PRONOUNS:[/bold]
        {"["}bold]JOB:[/bold]
    { paperwork-separator-hop }
    {"•"} Physical Description:
    {"•"} Relevant Medical Information:
    {"•"} Your interests:
    {"•"} Your skills:
    {"•"} Your past work experience:
    {"•"} In case of death:
    { paperwork-separator-hop }
    {"["}color=red]This document is not valid unless stamped by at least one of the required personnel.[/color]

    {"["}italic][color=gray]NOTE: The document is valid only if reliable information about the person is provided. The document is valid only if there is a stamp of the Head of the department. The accountable person confirms that this is a true documentation of the person's Identity. The accountable person confirms the transfer of the right to dispose of the life, health, inventory, soul, and assets of the accountable person to NanoTrasen Corporation. NanoTrasen Corporation is not responsible for loss of life/injury/loss of property/otherwise within the framework of Standard Operating Procedures, Space Law or other acts.[/color][/italic]

    {"["}bold] Head of Personnel Signature:[/bold]
    {"["}bold] Head of Personnel Stamp:[/bold]

paperwork-hop-formal-complaint =
    { paperwork-header-hop }
    { paperwork-separator-hop }
        {"["}head=2]Formal Complaint [/head]
    { paperwork-separator-hop }
        {"["}bold]SECTOR:[/bold] Floof Station
        {"["}bold]STATION:[/bold] {$stationName}
        {"["}bold]SHIFT TIME:[/bold] XX:XX
        {"["}bold]HEAD OF PERSONNEL:[/bold]
    { paperwork-separator-hop }
    {"["}head=3]Section A[/head]
    Complaintant's Name:               [color=#002AAF]Sample Person[/color]
                                                  ‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾
    Complaintant's Role:                  [color=#002AAF]Sample Role[/color]
                                                  ‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾
    { paperwork-separator-hop }
    {"["}head=3]Section B[/head]
    {"["}bold]Subject of Complaint:[/bold]
    \[  ] A Individual    \[  ] A Department    \[  ] Other
    {"["}italic]If other, please skip to Section C[/italic]

    Department of the Subject of Complaint:
    \[  ] Sci     \[  ] Engi  \[  ] Service         \[  ] Cargo        \[  ] Grey
    \[  ] Med  \[  ] Sec   \[  ] Command   \[  ] CentCom  \[  ] Other

    {"["}italic]If you are complaining about a person, please answer the following.[/italic]
    Name:    [color=#002AAF]Sample Person[/color]
                   ‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾
    {"["}italic]Please choose category(s) of misconduct the person or department has committed.[/italic]
    \[  ] Violation of Space Law      \[  ] Acting against NT interests
    \[  ] Destruction of Property   \[  ] Causing a disturbance
    \[  ] "Unspeakable Acts"          \[  ] Sabotage of equipment
    \[  ] Theft of Property               \[  ] Overly Bureaucratic
    \[  ] Overly Rude/Mean            \[  ] Not Bureaucratic Enough
    \[  ] Being Really Annoying
    \[  ] Other:    [color=#002AAF]Sample Problem[/color]
                   ‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾
    {"["}italic]Who Was Affected?[/italic]
    \[  ] Just Me    \[  ] A few people    \[  ] A department worth
    \[  ] Multiple Departments worth    \[  ] Whole Station
    { paperwork-separator-hop }
    {"["}head=3]Section C[/head]
    {"["}bold]Please provide a detailed explanation of the missconduct below, providing all details relevant to the story.[/bold]

    {"["}color=#1b67a5]‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾[/color]
    {"["}color=red][italic]Please either submit it to the Head of Personnel; if unavaliable, submit form to the Commanding Officer or to CentCom via fax for review. Please refrain from involving multiple commanding officers unless it is absolutely nessecary.[/italic][/color]

    {"["}bold]Signature of Complaintant:[/bold]

paperwork-hop-order-of-dismissal =
    { paperwork-header-hop }
    { paperwork-separator-hop }
        {"["}head=2]ORDER OF DISMISSAL [/head]
    { paperwork-separator-hop }
        {"["}bold]SECTOR:[/bold] Floof Station
        {"["}bold]STATION:[/bold] {$stationName}
        {"["}bold]SHIFT TIME:[/bold] XX:XX
        {"["}bold]ACCOUNTABLE PERSON:[/bold]
        {"["}bold]JOB:[/bold]
    { paperwork-separator-hop }
    {"•"} Dismissed employee:
    {"•"} The position of the dismissed employee:
    {"•"} The reason for the dismissal:
    { paperwork-separator-hop }

    {"["}italic]NOTE: This form of document is valid only for employees who do not have the status of Head of Department. The document must be sealed with the stamp of the department in which the dismissed employee worked, unless explicitly stated otherwise. After dismissal, the employee is obliged to receive the passenger's uniform, the passenger's headset and the passenger's PDA, having handed over all the professional inventory of the previous position.[/italic]

    {"["}color=red]This document is not valid unless stamped by at least one of the required personnel.[/color]

    {"["}bold]HEAD OF PERSONNELS STAMP:[/BOLD]

paperwork-hop-dismissal-of-department-head =
    { paperwork-header-hop }
    { paperwork-separator-hop }
        {"["}bold]DISMISSAL OF DEPARTMENT HEAD [/bold]
    { paperwork-separator-hop }
        {"["}bold]SECTOR:[/bold] Floof Station
        {"["}bold]STATION:[/bold] {$stationName}
        {"["}bold]SHIFT TIME:[/bold] XX:XX
        {"["}bold]COMMANDING OFFICER:[/bold]
        {"["}bold]ACCOUNTABLE PERSON:[/bold]
        {"["}bold]JOB:[/bold]
    { paperwork-separator-hop }
    {"•"} Name of the person being dismissed:
    {"•"} The position of the person being dismissed:
    {"•"} Reason for dismissal:
    {"•"} Additional comments [italic](issued by other heads of departments)[/italic]:
    { paperwork-separator-hop }

    {"["}color=red]This document is not valid unless stamped by at least one of the required personnel.[/color]

    {"["}italic]NOTE: The initialization of dismissal can only be carried out by the Commanding Officer or the Head of Personnel (with the permission of the Commanding Officer or Central Command). This document must have the signature of the dismissed person (except in cases of direct refusal to sign the order). This document is only valid if there is a stamp of the Commanding Officer, or the stamp of Central Command.[/italic]

    {"["}bold]Commanding Officer's Signature:
    Head Of Personnel's Signature:[/bold]

    Signature of the dismissed:


paperwork-hop-employment-application =
    { paperwork-header-hop }
    { paperwork-separator-hop }
        {"["}head=2]EMPLOYMENT APPLICATION[/head]
    { paperwork-separator-hop }
        {"["}bold]SECTOR:[/bold] Floof Station
        {"["}bold]STATION:[/bold] {$stationName}
        {"["}bold]SHIFT TIME:[/bold] XX:XX
        {"["}bold]ACCOUNTABLE PERSON:[/bold]
        {"["}bold]JOB:[/bold]
    { paperwork-separator-hop }
    {"•"} Desired employment:
    {"•"} Reasons why you should get this position:
    {"•"} Your interests:
    {"•"} Your skills:
    {"•"} Your past work experience:

    { paperwork-separator-hop }

    {"["}color=red]This document is not valid unless stamped by at least one of the required personnel.[/color]

    {"["}italic]NOTE: This document is valid only if reliable information about the person is provided. Providing false information is a reason for refusal of employment. This document is valid only if there is a stamp of the Head of the department where the accountable person is employed, unless explicitly stated otherwise. The accountable person confirms that they have all the necessary skills to work in the desired employment, is a healthy person. The accountable person confirms the transfer of the right to dispose of the life, health, inventory, soul and assets of the accountable person to NanoTrasen Corporation. NanoTrasen Corporation is not responsible for loss of life/injury/loss of property/otherwise within the framework of Standard Operating Procedures, Space Law or other acts.[/italic]

    {"["}bold] Head of Personnel Signature:[/bold]



paperwork-hop-restricted-equipment-permit =
    { paperwork-header-hop }
    { paperwork-separator-hop }
        {"["}head=2]Restricted Equipment Permit [/head]
    { paperwork-separator-hop }
        {"["}bold]SECTOR:[/bold] Floof Station
        {"["}bold]STATION:[/bold] {$stationName}
        {"["}bold]SHIFT TIME:[/bold] XX:XX
        {"["}bold]EQUIPMENT USER: [/bold]
        {"["}bold]EQUIPMENT TYPE: [/bold]
        {"["}bold]JOB: [/bold]
    { paperwork-separator-hop }
    {"•"}  Reasons for desired Equipment:
    {"•"}  Other Notes:
    { paperwork-separator-hop }
    {"["}color=red]This document gives [insert name] the ability to carry {"{stated gear}"} unhindered.[/color]

    {"["}color=gray]This Permit can be revoked as at a moment's notice if the applicable party violates Space Law in the use of said Equipment.[/color]

    {"["}color=red]This document must be signed and stamped by Head of Personnels & Head of Security to be considered valid.[/color]

    {"["}bold]Head of Personnels' Stamp & Signature:[/bold]
    {"["}bold]Head of Security Stamp & Signature:[/bold]


paperwork-hop-restricted-weapons-permit =
    { paperwork-header-hop }
    { paperwork-separator-hop }
        {"["}head=2]Restricted Weapons Permit [/head]
    { paperwork-separator-hop }
        {"["}bold]SECTOR:[/bold] Floof Station
        {"["}bold]STATION:[/bold] {$stationName}
        {"["}bold]SHIFT TIME:[/bold] XX:XX
        {"["}bold]WEAPON USER: [/bold]
        {"["}bold]WEAPON TYPE: [/bold]
        {"["}bold]JOB: [/bold]
    { paperwork-separator-hop }
    {"•"} Reasons for desired Equipment:
    {"•"} Other Notes:
    { paperwork-separator-hop }

    {"["}color=red]This document gives [insert name] the ability to carry L3 class armaments.[/color]

    {"["}color=gray]Class three armaments include but are not limited to: automatic weapons, combat explosives, and energy based melee weaponry.[/color]

    {"["}color=red]This document must be signed and stamped by Head of Personnels & Head of Security to be considered valid.[/color]

    {"["}bold]Head of Personnels' Stamp & Signature:[/bold]
    {"["}bold]Head of Security Stamp & Signature:[/bold]
