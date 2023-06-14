import numpy as np
import pandas as pd
from joblib import load
import sys
import os

def main():
    xdribbles_model = load(sys.argv[-1])
    features = np.array(sys.argv[1:-1]).reshape(1, -1)
    names = ["avgdribbles", "avgminutes", "avggoals", "avgassists", "avgtackles", "avgpossession", "grid0", "grid1"]
    print(xdribbles_model.predict(pd.DataFrame(features, columns=names))[0])

if __name__ == "__main__": main()