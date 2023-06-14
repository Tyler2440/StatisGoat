import random
from Model import Model


def main():
    limit = 10

    model = Model("xplayer")

    dataset = []
    all_players = model.get_data("players")
    num_per_player = 5

    for player_index in range(len(all_players)):
        if player_index % 10 == 0:
            print("player: " + str(player_index) + "/" + str(len(all_players)) + ", current rows: " + str(len(dataset)))

        player = all_players[player_index]

        pid = player["apiID"]

        all_player_matches = model.get_data("playerstats/player", {"pid": player["apiID"]})
        picked_matches = []

        if len(all_player_matches) > limit + num_per_player:
            while len(picked_matches) < num_per_player:
                index = random.randint(limit, len(all_player_matches))
                if index not in picked_matches:
                    picked_matches.append(index)
        elif len(all_player_matches) > limit:
            for ind in range(len(all_player_matches) - limit):
                picked_matches.append(limit + ind)

        for stats_index in range(len(picked_matches)):

            stats = all_player_matches[stats_index]

            playerdata = {}
            avg_stats = model.get_data("playerstats/avg",
                                       {
                                           "pid": pid,
                                           "limit": limit,
                                           "date": stats["datetime"]
                                       })
            avg_opp_stats = model.get_data("teamstats/avg",
                                        {
                                            "tid": stats["opponentid"],
                                            "limit": limit,
                                            "date": stats["datetime"]
                                        })
            avg_team_stats = model.get_data("teamstats/avg", 
                                            {
                                                "tid": stats["tid"],
                                                "limit": limit,
                                                "date": stats["datetime"]
                                            })

            if avg_stats["nummatches"] < limit: continue
            if avg_stats["avgminutes"] < 10: continue

            playerdata["avgminutes"] = avg_stats["avgminutes"]
            playerdata["avggoals"] = avg_stats["avggoals"]
            playerdata["avghsots"] = avg_stats["avgshots"]
            playerdata["avgassists"] = avg_stats["avgassists"]
            playerdata["avgpasses"] = avg_stats["avgpasses"]
            playerdata["avgpass_pct"] = avg_stats["avgpass_pct"]
            playerdata["avgtackles"] = avg_stats["avgtackles"]
            playerdata["avginterceptions"] = avg_stats["avginterceptions"]
            playerdata["avgduels"] = avg_stats["avgduels"]
            playerdata["avgfouls_drawn"] = avg_stats["avgfouls_drawn"]
            playerdata["avgfouls_committed"] = avg_stats["avgfouls_committed"]
            playerdata["avgpenalties_conceded"] = avg_stats["avgpenalties_conceded"]
            playerdata["avgred"] = avg_stats["avgred"]
            playerdata["avgyellow"] = avg_stats["avgyellow"]  
            playerdata["grid0"] = 0 if stats["grid"] is None else stats["grid"][:1]
            playerdata["grid1"] = 0 if stats["grid"] is None else stats["grid"][2:]         

            playerdata["avgteamconceded"] = avg_team_stats["avgconceded"]
            playerdata["avgteamfouls"] = avg_team_stats["avgfouls"]
            playerdata["avgteampossession"] = avg_team_stats["avgpossession"]
            playerdata["avgteampasses"] = avg_team_stats["avgpasses"]
            playerdata["avgteampassaccuracy"] = avg_team_stats["avgpass_pct"]
            playerdata["avgteamyellows"] = avg_team_stats["avgyellows"]
            playerdata["avgteamreds"] = avg_team_stats["avgreds"]
            
            playerdata["avgoppconceded"] = avg_opp_stats["avgconceded"]
            playerdata["avgoppfouls"] = avg_opp_stats["avgfouls"]
            playerdata["avgoppossession"] = avg_opp_stats["avgpossession"]
            playerdata["avgopppasses"] = avg_opp_stats["avgpasses"]
            playerdata["avgopppassaccuracy"] = avg_opp_stats["avgpass_pct"]
            playerdata["avgoppyellows"] = avg_opp_stats["avgyellows"]
            playerdata["avgoppreds"] = avg_opp_stats["avgreds"]

            playerdata["fouls"] = stats["fouls_committed"]
            playerdata["yellow"] = stats["yellow"]
            playerdata["red"] = stats["red"]

            dataset.append(playerdata)

    model.load_to_csv("xfoulplay.csv", dataset)


if __name__ == "__main__": main()

