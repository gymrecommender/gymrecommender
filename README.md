# Software engineering
Gyms recommender

# Description of the project
Our app will help to find the best gym options for people with different priorities

Since "the best" is extremely vague and unspecific, we have to define the criteria that will make "the best" measurable
- Each user will be able to choose a specific location for which he/she/them want(s) to receive gyms recommendations both manually and automatically. 
- Each user will be able to specify a time interval for which the recommendation should be created in order to get the most precise possible travelling time and cost information. This will help users to get the options that are tailored to a specific situation (going to the gym from home, after work, anything imaginable)
- Each gym in the city will have a rating assigned to it based on the following parameters 
  - travelling time from the chosen location
  - travelling cost (public transport only) from the chosen location
  - membership's price
  - congestion rating of the gym
  - overall rating of the gym
- Each user will be able to define the personal importance of each aforementioned criterion
- Based on the provided information, the app will recommend the top 5 gyms that satisfy the set criteria the best. Each of these gyms will be shown on a Google Map as a result with respective data for all 5 parameters mentioned above

Every user will be able to create his/her/their own user account where all the previous requests will be shown in the history. Besides this, each logged-in user will have an opportunity to bookmark specific gyms in order to get notifications if any information regarding the favoured gyms changes.
Each anonymous and logged-in user will be able to rate the gym and the congestion of the gym in particular

Each gym account will be able to manage data related to the gyms that are attached to it, including membership price and working hours.

Before a certain gym account can have a gym attached to it and receives access to editing its data, an administrator must approve this binding

# Functional requirements
Users = both authenticated and anonymous users (if not specified in the context)
| Code | Description | Priority |
| ---- | ----------- | -------- |
|  F1    |  The system must support OAuth 2.0 authentication using Firebase           |  Medium        |
|  F2    |  Anonymous users must have access to all the functionalities that are not bound to specific account type functionalities          |  High        |
|  F3    |  Users must be able to create a local account           |  Medium     |   
|  F4    |  Users must be able to delete their account           |  Low        |
|  F5    |  Users must be able to log in with local accounts or Google accounts           | Medium         |
|  F6    |  Users must be able to log in only with the type of account (local or Google) they signed up with           |  Medium        |
|  F7    |  Username must serve as a unique identifier of a user           |  Medium        |
|  F8    |  Users must be able to manage their own accounts           | Medium         |
|  F9    |  Each user must have the ability to bookmark gyms regardless of their attachment status to a Gym account           |  Medium        |
|  F10   |  When a bookmarked gym's information changes, all the users that have this gym bookmarked must receive a notification in their accounts          |  Medium        |
|  F11   |  Users must have a personal dashboard displaying bookmarked gyms, the history of requests and notifications           |  Medium        |
|  F12   |  Gym owners must be able to request "ownership" of specific gyms           |  High        |
|  F13   |  Gym accounts must be able to manage the requested gyms only after approval from an administrator           |  Medium        |
|  F14   |  It must be possible to create new gym accounts           |  High        |
|  F15   |  Gym accounts must be uniquely identified by their respective usernames         |  High        |
|  F16   |  Each gym must be managed by at most one account           |  High        |
|  F17   |  Each gym account must be able to manage multiple gyms           |  High        |
|  F18   |  Each gym account must be able to delete its account           |  Low        |
|  F19   |  Gym accounts must not be able to delete any gyms from the system regardless of the ownership status           |  High        |
|  F20   |  Gym accounts must be able to mark a gym as temporarily or permanently unavailable           |  Low        |
|  F21   |  Gym account-managed data must override Google-provided data for the gym except for the overall and congestion ratings          |  High        |
|  F22   |  Overall and congestion ratings of a gym retrieved from Google must be combined with the respective local ratings           |  High        |
|  F23   |  Administrator accounts must be created only by other administrators (a respective section in the administrator's account)           |  Medium        |
|  F24   |  Administrator accounts must be able to approve or decline gym ownership requests on a per-gym basis          |  Medium        |
|  F25   |  Each user, gym or administrator account has access only to the openly available information and information specific to his/her/their personal account          |  High        |
|  F26   |  Users must have the option to have their geolocation automatically retrieved           |  High        |
|  F27   |  Geolocation of users must be retrieved only upon explicit request           | High         |
|  F28   |  Users must be able to choose the location by manually selecting a marker on a map           |  High        |
|  F29   |  User must be able to choose the location by searching via providing a search bar with a specific address           |  High        |
|  F30   |  Users must be able to specify a desired travelling time period for which they want to receive recommendations           |   Medium       |
|  F31   |  The recommendation of the gyms must be based on the following **criteria**: travelling time from the chosen location, travelling price from the selected location, gym overall rating, gym congestion rating and gym's membership price|    High |
|  F32   |  Users must be able to adjust the importance of all the recommendation-based-on criteria via a respective slider           |  High        |
|  F33   |  Users must explicitly trigger the search for gyms to be retrieved and recommendations to be formed       |  High        |
|  F34   |  Each user must be able to submit only one request per 2 minutes           |  High        |
|  F35   |  The request button becomes hidden until the next request is available and a countdown timer appears in its place           |  Medium        |
|  F36   |  The system must analyze all gyms in the implicitly (via a marker) selected city          |  High        |
|  F37   |  The system must generate a list of the top 5 gyms taking into account the importance of each criterion specified by the user           |  High        |
|  F38   |  Congestion ratings should be calculated based on the estimated arrival time if provided, otherwise, an average congestion rate must be used           |  Medium        |
|  F39   |  A separate top 3 recommendation list must be formed if there are gyms that lack information for certain criteria and the missing criteria must be highlighted           |  Low        |
|  F40   |  Each recommended gym must be displayed on a map         |  High        |
|  F41   |  Regular and alternate (top 3) recommended gyms must be displayed on a map in different colours with respect to each other         |  High        |
|  F42   |  Both regular and alternate (top 3) gym ratings must be displayed next to the resulting map           |   High       |
|  F43   |  Only authenticated users can contribute to the overall and congestion ratings           |  High        |
|  F44   |  Gym congestion must be rated by selecting an appropriate category for two parameters (average waiting time for machines or spaces and how crowded the gym feels) and specifying the user's time of visit in order for the review to be submitted         |   High       |
|  F45   |  Authenticated users must be able to rename and browse through previous requests at any time        |  Medium        |
|  F46   |  When browsing through the result of the requests (including the ones in the history) the parameters of the request and the respective values of each of the criteria alongside with all information available for the gym must be visible        |  High        |
|  F47   |  If any gym in the history changes its information after the recommendation has been composed, an appropriate note must be displayed next to the respective gym upon selection of the request it belongs to          |  Medium        |


### Non-Functional Requirements
| Code | Description |
| ---- | ----------- |
|NF1| The system should return gym recommendations within 5 seconds after the user makes a request provided the internet connection of the user is reliable |
|NF2| The system should update gym information (e.g. membership price of the bookmarked gym) and deliver notifications within 10 seconds of the change |
|NF3| User data must be encrypted during transition. Sensitive data like passwords must also be stored encrypted. |
|NF4| Authentication tokens must expire after 30 minutes of inactivity and users must be required to re-authenticate |
|NF5| Only authorized administrators should be able to manage gym ownership requests or create administrator accounts (via secure backend processes) |
|NF6| Notifications regarding gym updates must be reliably delivered to at least 99.5% of subscribed users |
|NF7| The user interface must be intuitive and responsive, with page load times not exceeding 2 seconds under normal conditions |
|NF8| The system must be accessible to users with disabilities, complying with WCAG 2.1 AA standards |
|NF9| The system codebase must follow standard design patterns and be modular to facilitate future updates and maintenance |
|NF10| The system must provide comprehensive logging of errors and events for system administrators to troubleshoot effectively |
|NF11| The system must have clear documentation for all APIs and user-facing features to support future development and debugging |
|NF12| Any changes to gym or user data must be reflected consistently across all views upon the next request of the gym's information. |

# Technology
Google Maps API  
Google Geocode API  
Google Places API  
Google Directions API  
Django  
React  
PostgreSQL  
Firebase  
Firebase Cloud Messaging  

# Installation
The application will be deployed at Render (https://render.com/)

# Team members
Nikla Magyar  
Mia Šagovac  
Josipa Jagodić  
Egor Shevtsov  
Ivan Ivica
Ilija Rotan

# Contribution
Nikla Magyar - Back-end  
Ivan Ivica - Back-end
Mia Šagovac - Front-end + Design + Layout + UML
Josipa Jagodić - Front-end  
Egor Shevtsov - Front-end + Database + Design + Layout
Ilija Rotan - Back-end



# 📝 Code of Conduct [![Contributor Covenant](https://img.shields.io/badge/Contributor%20Covenant-2.1-4baaaa.svg)](CODE_OF_CONDUCT.md)
As students, you are surely familiar with the minimum acceptable behaviour defined in [CODE OF CONDUCT FOR STUDENTS OF THE FACULTY OF ELECTRICAL ENGINEERING AND COMPUTER SCIENCES OF THE UNIVERSITY OF ZAGREB](https://www.fer.hr/_download/repository/Kodeks_ponasanja_studenata_FER-a_procisceni_tekst_2016%5B1%5D.pdf), and additional instructions for teamwork in the subject [Program Engineering] (https://www.fer.hr).
We expect you to abide by the [IEEE Code of Ethics](https://www.ieee.org/about/corporate/governance/p7-8.html) which has an important educational function with the purpose of setting the highest standards of integrity, responsible behaviour and ethical behaviour in professional activities. Thus, the professional community of software engineers defines general principles that define moral character, making important business decisions and establishing clear moral expectations for all members of the community.

A code of conduct is a set of enforceable rules that serve to clearly communicate expectations and requirements for community/teamwork. It clearly defines obligations, rights, unacceptable behaviour and corresponding consequences (in contrast to the code of ethics). One of the widely accepted codes of conduct for working in the open-source community is given in this repository.

# 📝 License
Valid (1)
[![CC BY-NC-SA 4.0][cc-by-nc-sa-shield]][cc-by-nc-sa]

This repository contains open educational resources (eng. Open Educational Resources) and is licensed according to the rules of the Creative Commons license, which allows you to download the work, share it with others with
provided that you mention the author, do not use it for commercial purposes and share it under the same conditions [Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License HR][cc-by-nc-sa].
>
> ### Note:
>
> All packages are distributed under their own licenses.
> All materials used (images, models, animations, ...) are distributed under their own licenses.

[![CC BY-NC-SA 4.0][cc-by-nc-sa-image]][cc-by-nc-sa]

[cc-by-nc-sa]: https://creativecommons.org/licenses/by-nc/4.0/deed.hr
[cc-by-nc-sa-image]: https://licensebuttons.net/l/by-nc-sa/4.0/88x31.png
[cc-by-nc-sa-shield]: https://img.shields.io/badge/License-CC%20BY--NC--SA%204.0-lightgrey.svg

Original [![cc0-1.0][cc0-1.0-shield]][cc0-1.0]
>
>COPYING: All the content within this repository is dedicated to the public domain under the CC0 1.0 Universal (CC0 1.0) Public Domain Dedication.
>
[![CC0-1.0][cc0-1.0-image]][cc0-1.0]

[cc0-1.0]: https://creativecommons.org/licenses/by/1.0/deed.en
[cc0-1.0-image]: https://licensebuttons.net/l/by/1.0/88x31.png
[cc0-1.0-shield]: https://img.shields.io/badge/License-CC0--1.0-lightgrey.svg

### Repository Licensing References
