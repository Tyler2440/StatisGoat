# StatisGoat

Team: Tyler Allen, Ethan Bork, Austin Kelley, Wyatt Light, Gregorius Sukiman 

Designed for sports bettors and passionate fans of the beautiful game, StatisGoat will attempt to centralize the resources users need to make reliable, informed predictions on matches from Europe’s most talented leagues. Lineup displays and player-specific stat visualizations will inspire users to challenge their bias and make scientific predictions on match outcomes and events. Comparing these predictions to those of StatisGoat’s human-inspired machine learning algorithms will once again push users to challenge their understanding of the game and integrate what their predictions might have failed to consider. With their updated knowledge, users can engage in passionate discussions about their predictions, or the ones StatisGoat provides them, or head over to their sports book of choice and more accurately evaluate which bets are the best value for money.

StatisGoat will deliver its visualizations and predictions to users via a website. On the homepage, users will find a seven day calendar where, for each day, they can view the matches - separated by league - for which we are offering predictions. Live matches will be displayed alongside their live score, others will be displayed alongside their start time. Matches will also be displayed alongside their outcome prediction, presented as a balance of power bar; this prediction will be live and dependent upon the score of the match. Lastly on the home page, users will be able to view the top performers for the selected day compared against the model’s top predicted performers.

Clicking on one of the presented games will direct users to the game page where they will view team lineups overlaid with the field and game-specific performer comparisons. Clicking on any player in the lineup will bring users to the player-specific page where the bulk of StatisGoat’s visualizations will be presented. Users will be able to toggle a variety of player stat visualizations like shots, goals, passes, assists, tackles, fouls, player heatmaps, etc., and will be able to set the timeline from which the stats are gathered from the current game, to the current seasons, to the seasons past. Each player page will also come with player specific predictions like expected goals, expected assists, dribbles, fouls, etc, to provide users with extensive predictions on both match outcomes and events.

StatisGoat will also offer social media capabilities throughout the home, game, and player pages. Users will be able to react to performances and predictions, in a similar way to reactions in instagram and snapchat, to quickly indicate to other users which predictions they think are hot, and which they think are cold. Each game page will also come with a chat forum where users can discuss the game, and compare their predictions to the model’s.

The main specific of the idea that is still up for discussion is the scope of matches for which StatisGoat will offer predictions, specifically, which leagues and the number of leagues that we will study. The current plan is to study Europe’s “top five” leagues along with the Champions League, but this will largely depend on what leagues we can easily gather data from. It might make sense to include the MLS in the project, since the StatisGoat team has already met with Real Salt Lake’s lead analyst to gather invaluable insight on the project’s implementation.


Platforms: Web Application hosted using EC2

IDE: Visual Studio (C#, HTML, Javascript) and Visual Studio Code(Python)

Outside Library: 
- sklearn --> Library to train and predict models by using the algorithms in the library
- joblib --> Library to dump and load the trained models
- Bcrypt --> Libray to hash the password

