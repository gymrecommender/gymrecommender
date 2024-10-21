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
1. Authentication
   - Oauth2.0 authentication with the help of Firebase (local accounts + Google only)
   - Email addresses are unique identifiers of the users
   - Anonymous users are identified by their IP addresses
2. Accounts
   - Each user has an opportunity to create their own account
   - Each user has an opportunity to bookmark gyms. This way if any parameter of the gym is changed, the user gets a respective notification in his account
   - Gym accounts request the "ownership" of particular gyms in order to change/set the respective data (membership price, working hours, etc). Any type of managing of the data is allowed only after the "ownership" is approved by an administrator account.
   - Each gym can be managed only by one gym account
   - Each gym account can manage multiple gyms
   - A gym account can indicate that the gym under its management is not available for a set period of time or closed in general. This information will affect the formation of the rating
   - An administrator account has the power to accept or decline the requested attachment of the gym(s) to a certain account. Each gym is accepted or declined separately from all the others
   - Parameters set via gym's account have a higher priority than the parameters retrieved from Google
3. Geolocation
   - Automatic retrieval of the user's geolocation upon an explicit request of the user
   - Manual selection of the location via selecting a marker on the map
   - Manual selection of the location via providing the address to the respective search bar
4. Setting parameters of the requests
   - Each user has an opportunity to specify a desired time period during which he is expected to travel to the gym
   - Travelling time (from the selected location), travelling price (public transport from the selected location), gym membership price, user's overall rating and user's congestion rating are the criteria based on which the recommendation is made. Each user has an opportunity to assign importance to each of the aforementioned criteria via a respective slider.
5. Gyms retrieval
   - Retrieval of the gyms is triggered explicitly by the user, not automatically upon selection of the marker
   - Each user is allowed to make only one request per minute. After that the request-triggering button becomes hidden and the respective loader-counter showing the time until the next request is possible appears on its place 
   - All the gyms of the city are retrieved alongside their information if available (price, rating, working hours, etc)
6. Comprising gyms recommendation
   - Based on the aforementioned criteria and parameters of the request, each user gets the top 5 gyms
   - Congestion rating is calculated based on the estimated arrival time if the time parameter provided by the user in the request. Otherwise, an average congestion is used as an estimate
   - If some gyms do not have information for some criteria, they form another top 3 (if possible) rating that is shown separately from the regular rating with undefined criteria highlighted
   - Each of the recommended gyms is depicted on the map via a marker. Each rating has its own marker colour
   - Both of the ratings are shown to the right of the aforementioned map
7. Ratings
   - Each user has an opportunity to leave an overall rating of a gym. The discrete rating must be in a [1, 5] range
   - Each user has an opportunity to rate congestion in the gym. A user will have to choose one of the provided options in each respective field
     - average waiting time for a machine or space (5 options to choose from)
     - how crowded the gym feels (5 options to choose from)
     - time of visit (a time field)
8. History
   - Each user’s account has a history of requests that he/she/them can rename and browse through at any time
   - If any information regarding any gym in history has been changed, an appropriate visible note will be shown next to the respective gym


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
# Team members
Nikla Magyar  
Mia Šagovac  
Josipa Jagodić  
Egor Shevtsov  
Ivan Ivica  

# Contribution
Nikla Magyar - Back-end  
Ivan Ivica - Back-end + UML
Mia Šagovac - Front-end + Design + Layout  
Josipa Jagodić - Front-end  
Egor Shevtsov - Front-end + Database  



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
