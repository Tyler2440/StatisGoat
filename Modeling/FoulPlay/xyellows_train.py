import numpy as np
np.set_printoptions(threshold=20)
import pandas as pd
from sklearn.ensemble import RandomForestRegressor
from sklearn.linear_model import LinearRegression
from matplotlib import pyplot
from joblib import dump, load
import csv

def make_model(train_x, train_y, model): return model.fit(train_x, train_y)

def rem_neg(predicts): predicts[predicts < 0] = 0

def sse(test_y, predicts): return sum([(test_y.tolist()[i] - predicts[i])**2  for i in range(len(predicts))])

def coef_det(test_x, test_y, model): return model.score(test_x, test_y)

def percent_correct(test_y, tol, predicts): return sum([1 if abs(test_y.tolist()[i] - predicts[i]) <= tol else 0 for i in range(len(predicts))]) / len(predicts)

def percent_correct_scored(test_y, tol, predicts):
    scored = 0
    correct = 0
    for i in range(len(test_y.tolist())):
        if test_y.tolist()[i] < 1: continue
        scored += 1
        if abs(test_y.tolist()[i] - predicts[i]) <= tol: correct += 1
    return correct / scored

def main():

    col_names = ["avgminutes", "avggoals", "avgshots", "avgassists", "avgpasses", "avgpass_pct", "avgtackles", "avginterceptions",
                 "avgduels", "avgfouls_drawn", "avgfouls_committed", "avgpenalties_conceded", "avgred", "avgyellow", "grid0", "grid1", 
                 "avgteamconceded", "avgteamfouls", "avgteampossession", "avgteampasses", "avgteampasspct", "avteamyellows", "avgteamred", 
                 "avgoppconceded", "avgoppfouls", "avgopppossession", "avgopppasses", "avgopppasspct", "avgoppyellows", "avgoppred",
                 "fouls", "yellow", "red"]

    xyellows_data = pd.read_csv("xfoulplay.csv", names=col_names, header=None)
    xyellows_data.drop(["fouls", "red"], axis = 1, inplace = True)

    # Spliting the model into 80% training and 20% test
    train_rows = int(xyellows_data.shape[0] * 0.8)
    xyellows_train = xyellows_data.iloc[:train_rows,:]
    xyellows_test = xyellows_data.iloc[train_rows:xyellows_data.shape[0],:]
    xyellows_train_x = xyellows_train.iloc[:,:-1]
    xyellows_train_y = xyellows_train.iloc[:,-1]
    xyellows_test_x = xyellows_test.iloc[:,:-1]
    xyellows_test_y = xyellows_test.iloc[:,-1]

    print("Models trained on features set: %s" % xyellows_data.columns.values)

    xyellows_linear = make_model(xyellows_train_x, xyellows_train_y, LinearRegression())
    print("Stats for Linear Regression Model")
    linear_predict = xyellows_linear.predict(xyellows_test_x)
    print("Predicted xYellows on Test Data: %s" % linear_predict)
    print("Max xYellow Prediction: %s" % max(linear_predict))

    rem_neg(linear_predict)

    linear_sse = sse(xyellows_test_y, linear_predict)
    print("Sum SSE on Test Data: %f" % linear_sse)
    print("Avg SSE on Test Data: %f" % (linear_sse / len(linear_predict)))
    print("Coef of Determination on Test Data: %f" % (coef_det(xyellows_test_x, xyellows_test_y, xyellows_linear)))
    print("Percent Correct within Range %f: %f" % (0.1, percent_correct(xyellows_test_y, 0.1, linear_predict)))
    print("Percent Correct (When Yellow) within Range %f: %f" % (0.7, percent_correct_scored(xyellows_test_y, 0.7, linear_predict)))

    xyellows_randomforest = make_model(xyellows_train_x, xyellows_train_y, RandomForestRegressor())
    print("Stats for Random Forest Model")
    rf_predict = xyellows_randomforest.predict(xyellows_test_x)
    print("Predicted xYellows on Test Data: %s" % rf_predict)
    print("Max xYellow Prediction: %s" % max(rf_predict))

    rem_neg(rf_predict)

    rf_sse = sse(xyellows_test_y, rf_predict)
    print("Sum SSE on Test Data: %f" % rf_sse)
    print("Avg SSE on Test Data: %f" % (rf_sse / len(rf_predict)))
    print("Coef of Determination on Test Data: %f" % (coef_det(xyellows_test_x, xyellows_test_y, xyellows_randomforest)))
    print("Percent Correct within Range %f: %f" % (0.1, percent_correct(xyellows_test_y, 0.1, rf_predict)))
    print("Percent Correct (When Yellow) within Range %f: %f" % (0.7, percent_correct_scored(xyellows_test_y, 0.7, rf_predict)))

    dump(xyellows_randomforest, 'xyellows_model.joblib')

    # # get importance
    # importance = xyellows_randomforest.feature_importances_
    # # summarize feature importance
    # for i,v in enumerate(importance):
    #  print('Feature: %s, Score: %.5f' % (col_names[i],v))
    # # plot feature importance
    # pyplot.bar([x for x in range(len(importance))], importance)
    # pyplot.show()

if __name__ == "__main__": main()