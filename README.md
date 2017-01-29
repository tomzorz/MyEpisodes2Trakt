# MyEpisodes2Trakt
This tool migrates your TV shows to Trakt.tv from MyEpisodes.com.

## ~ what does it do exactly?

It extracts the shows from your myepisodes page, finds IMDB IDs for the shows with the omdb api, then finally looks up these shows on trakt and adds them to your collection and watch history. (episode specific information is ignored, shows are marked as watched/collected as a whole) There isn't really any error handling for anything out of the ordinary except the following:

* it'll skip shows that are already added in trakt
* if something wasn't found (either with omdb or on trakt) it'll list it at the end so you can migrate those by hand

## ~ some assembly required

1. grab a copy of the latest and greatest visual studio
2. clone this git repo somewhere
3. open _TraktImporter.sln_
4. go to [https://trakt.tv/oauth/applications](https://trakt.tv/oauth/applications) and create a new API app (check both boxes, use the zapp thingy in the sample as redirect url)
5. copy the resulting clientId and clientSecret into the marked location in _Program.cs_
6. go to [http://www.myepisodes.com/timewasted/](http://www.myepisodes.com/timewasted/) and copy **all** the data from the table into a text file (skip header and summary)
7. save that file somewhere and paste its path into _Program.cs_ at the marked location
8. hit F5 and follow the instructions (you'll need to paste the OAUTH pin)
9. let the thing do it's job, might take a few seconds-minutes depending on how much life you have
9. ???
10. profit

### Please ignore below, these are here so more people can find this

migrate myepisodes to trakt trakt.tv automatic copy import export csv time wasted shows series tv episodes collection c# script batch

