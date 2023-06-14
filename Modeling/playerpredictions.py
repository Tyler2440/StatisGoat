def warn(*args, **kwargs):
    pass
import warnings
warnings.warn = warn

import numpy as np
import pandas as pd
from joblib import dump, load
import sys
import os
from enum import Enum
from sklearn.preprocessing import PolynomialFeatures


class Helper(Enum):
    SHOTS = ("Shots/xshots_model.joblib",
             ["avgshots", "avgshots_on_goal", "avggoals", "avgminutes", "avgdribbles", "avgdribbles_won", "pct_scored",
              "avgteam_scored", "avgopp_conceded", "avgteam_shots", "avgteam_shots_on_goal", "avgteam_possession",
              "height"])
    GOALS = ("Goals/xg_model.joblib",
             ["avgGoals", "psctScored", "avgShots", "avgShotsOnGoal", "avgMinutes", "avgDribbles", "avgDribblesWon",
              "avgTeamScores", "avgOppConceded", "height", "width"])
    ASSISTS = ("Assists/xassists_model.joblib",
               ["avgPasses", "avgKeyPasses", "avgPass_Percentage", "avgAssists", "avgDribbles", "avgDribblesWon",
                "avgMinutes", "Percent_Assisted", "avgTeamScored", "avgOppConceded", "avgTeamShots", "avgTeamShotsOG",
                "avgPossesion", "height", "width"])
    SAVES = ("Saves/xsaves_model.joblib",
             ["avgSaved", "avgBlocks", "avgPenalties_saved", "avgPenalties_conceded", "avgMinutes", "avgTeamconceded",
              "avgTeamPossesion", "AvgOppshots", "AvgOppshots_OG", "AvgOppscored", "height", "width"])
    PASSES = ("Passes/xpasses_model.joblib",
              ["avgPasses", "avgPasses_acc", "avgPass_pct", "avgMinutes", "avgTeam_passes", "avgTeamPasses_act",
               "AvgTeamPossesion", "AvgOppPossesion", "AvgOpp_passes_act", "height", "width"])
    TACKLES = ("Defensive/xtack_model.joblib",
               ["avgoppconceded", "avgminutes", "avgpasses", "avgtackles", "avgblocks", "avginterceptions", "avgduels",
                "avgduels_won", "avgfouls_committed", "avgyellow", "avgred", "avgpossession", "grid0", "grid1"])
    INTERCEPTIONS = ("Defensive/xint_model.joblib",
                     ["avgoppconceded", "avgminutes", "avgpasses", "avgtackles", "avgblocks", "avginterceptions",
                      "avgduels", "avgduels_won", "avgfouls_committed", "avgpossession", "grid0", "grid1"])
    DRIBBLES = ("Dribbles/xdribbles_model.joblib",
                ["avgdribbles", "avgminutes", "avggoals", "avgassists", "avgtackles", "avgpossession", "grid0",
                 "grid1"])
    FOULS = ("FoulPlay/xfouls_model.joblib",
             ["avgminutes", "avggoals", "avgshots", "avgassists", "avgpasses", "avgpass_pct", "avgtackles",
              "avginterceptions", "avgduels", "avgfouls_drawn", "avgfouls_committed", "avgpenalties_conceded", "avgred",
              "avgyellow", "grid0", "grid1", "avgteamconceded", "avgteamfouls", "avgteampossession", "avgteampasses",
              "avgteampasspct", "avteamyellows", "avgteamred", "avgoppconceded", "avgoppfouls", "avgopppossession",
              "avgopppasses", "avgopppasspct", "avgoppyellows", "avgoppred"])
    YELLOWS = ("FoulPlay/xyellows_model.joblib", FOULS[1])
    REDS = ("FoulPlay/xreds_model.joblib", FOULS[1])
    RATING = ("Rating/xrating_model.joblib",
              ["shots", "goals", "assists", "saves", "passes", "tackles", "interceptions", "dribbles", "fouls",
               "yellows", "reds"])


def main():
    env = sys.argv[1]
    if env == "Development":
        os.chdir("../../../../")
    os.chdir("Modeling/")

    # Get all unique data from arguments
    (g0, g1, p_dribbles, p_dribbles_won, p_passes, p_pass_acc, p_key_pass, p_pass_percent, p_assists, p_assist_percent,
     p_shots, p_shots_on_goal, p_goals, p_score_percent, p_saved, p_tackles, p_blocks, p_interceptions, p_duels,
     p_duels_won, p_fouls_drawn, p_fouls_committed, p_penalties_conceded, p_penalties_saved, p_yellows, p_reds,
     p_minutes,

     t_passes, t_passes_acc, t_pass_percent, t_shots, t_shots_on_goal, t_scored, t_conceded, t_fouls, t_yellows, t_reds,
     t_possession,

     o_passes, o_passes_acc, o_pass_percent, o_shots, o_shots_on_goal, o_scored, o_conceded, o_fouls, o_yellows, o_reds,
     o_possession) = sys.argv[2:]

    # Feature sets for all the models
    f_shots = [p_shots, p_shots_on_goal, p_goals, p_minutes, p_dribbles, p_dribbles_won, p_score_percent, t_scored,
               o_conceded, t_shots, t_shots_on_goal, t_possession, g0]
    f_goals = [p_goals, p_score_percent, p_shots, p_shots_on_goal, p_minutes, p_dribbles, p_dribbles_won, t_scored,
               o_conceded, g0, g1]
    f_assists = [p_passes, p_key_pass, p_pass_percent, p_assists, p_dribbles, p_dribbles_won, p_minutes,
                 p_assist_percent, t_scored, o_conceded, t_shots, t_shots_on_goal, t_possession, g0, g1]
    f_saves = [p_saved, p_blocks, p_penalties_saved, p_penalties_conceded, p_minutes, t_conceded, t_possession, o_shots,
               o_shots_on_goal, o_scored, g0, g1]
    f_passes = [p_passes, p_pass_acc, p_pass_percent, p_minutes, t_passes, t_passes_acc, t_possession, o_possession,
                o_passes_acc, g0, g1]
    f_tackles = [o_conceded, p_minutes, p_passes, p_tackles, p_blocks, p_interceptions, p_duels, p_duels_won,
                 p_fouls_committed, p_yellows, p_reds, t_possession, g0, g1]
    f_interceptions = [o_conceded, p_minutes, p_passes, p_tackles, p_blocks, p_interceptions, p_duels, p_duels_won,
                       p_fouls_committed, t_possession, g0, g1]
    f_dribbles = [p_dribbles, p_minutes, p_goals, p_assists, p_tackles, t_possession, g0, g1]
    f_fouls = f_yellow = f_red = [p_minutes, p_goals, p_shots, p_assists, p_passes, p_pass_acc, p_tackles,
                                  p_interceptions, p_duels, p_fouls_drawn, p_fouls_committed, p_penalties_conceded,
                                  p_reds, p_yellows, g0, g1, t_conceded, t_fouls, t_possession, t_passes,
                                  t_pass_percent,
                                  t_yellows, t_reds, o_conceded, o_fouls, o_possession, o_passes, o_pass_percent,
                                  o_yellows, o_reds]

    results = [
        run(Helper.SHOTS.value, f_shots),
        run2(Helper.GOALS.value, f_goals),
        run2(Helper.ASSISTS.value, f_assists),
        run(Helper.SAVES.value, f_saves),
        run(Helper.PASSES.value, f_passes),
        run(Helper.TACKLES.value, f_tackles),
        run(Helper.INTERCEPTIONS.value, f_interceptions),
        run(Helper.DRIBBLES.value, f_dribbles),
        run(Helper.FOULS.value, f_fouls),
        run(Helper.YELLOWS.value, f_yellow),
        run(Helper.REDS.value, f_red)]

    results.append(run(Helper.RATING.value, results))

    print(results.__str__()[1:-1])


def run(data, features):
    model = load(data[0])
    names = data[1]
    features = np.array(features).reshape(1, -1)

    result = model.predict(pd.DataFrame(features, columns=names))[0]
    return 0 if result < 0 else result


def run2(data, features):
    model = load(data[0])
    names = data[1]
    features = pd.DataFrame(np.array(features).reshape(1, -1), columns=names)

    result = model.predict(PolynomialFeatures(degree=3, include_bias=False).fit_transform(features))[0]
    return 0 if result < 0 else result


if __name__ == "__main__": main()
