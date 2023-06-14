import numpy as np
import pandas as pd
# from sklearn.ensemble import RandomForestRegressor
# from sklearn.linear_model import LinearRegression
# from matplotlib import pyplot
from joblib import dump, load
import sys
import os

def main():
    # print("Number of Args: %s" % len(sys.argv))
    # print("Argv: %s" % str(sys.argv))
    path = os.path.abspath(sys.argv[-1])
    xsaves_model = load(path)
    features = np.array(sys.argv[1:-1]).reshape(1, -1)
    names = ["avgSaved","avgBlocks","avgPenalties_saved","avgPenalties_conceded","avgMinutes","avgTeamconceded",
                "avgTeamPossesion","AvgOppshots","AvgOppshots_OG","AvgOppscored","height","width"]
    print(xsaves_model.predict(pd.DataFrame(features, columns=names))[0])

if __name__ == "__main__": main()