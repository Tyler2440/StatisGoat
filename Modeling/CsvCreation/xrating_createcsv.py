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

            playerdata["shots"] = stats["shots"]
            playerdata["goals"] = stats["goals"]
            playerdata["assists"] = stats["assists"]
            playerdata["saves"] = stats["saves"]
            playerdata["passes"] = stats["passes"]
            playerdata["tackles"] = stats["tackles"]
            playerdata["interceptions"] = stats["interceptions"]
            playerdata["dribbles"] = stats["dribbles"]
            playerdata["fouls"] = stats["fouls_committed"]
            playerdata["yellows"] = stats["yellow"]
            playerdata["reds"] = stats["red"]

            playerdata["rating"] = stats["rating"]

            dataset.append(playerdata)

    model.load_to_csv("xrating.csv", dataset)


if __name__ == "__main__": main()

