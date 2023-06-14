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
    col_names = ["shots", "goals", "assists", "saves", "passes", "tackles", "interceptions", "dribbles", "fouls", "yellows", "reds", "rating"]
    xrating_data = pd.read_csv("xrating.csv", names=col_names, header=None)
    # xrating_data.drop(["saves"], axis = 1, inplace = True)

    # Spliting the model into 80% training and 20% test
    train_rows = int(xrating_data.shape[0] * 0.8)
    xrating_train = xrating_data.iloc[:train_rows,:]
    xrating_test = xrating_data.iloc[train_rows:xrating_data.shape[0],:]
    xrating_train_x = xrating_train.iloc[:,:-1]
    xrating_train_y = xrating_train.iloc[:,-1]
    xrating_test_x = xrating_test.iloc[:,:-1]
    xrating_test_y = xrating_test.iloc[:,-1]

    print("Models trained on features set: %s" % xrating_data.columns.values)

    xrating_linear = make_model(xrating_train_x, xrating_train_y, LinearRegression())
    print("Stats for Linear Regression Model")
    linear_predict = xrating_linear.predict(xrating_test_x)
    print("Predicted xRating on Test Data: %s" % linear_predict)

    rem_neg(linear_predict)

    linear_sse = sse(xrating_test_y, linear_predict)
    print("Max xRating: %s" % max(linear_predict))
    print("Sum SSE on Test Data: %f" % linear_sse)
    print("Avg SSE on Test Data: %f" % (linear_sse / len(linear_predict)))
    print("Coef of Determination on Test Data: %f" % (coef_det(xrating_test_x, xrating_test_y, xrating_linear)))
    print("Percent Correct within Range %f: %f" % (0.5, percent_correct(xrating_test_y, 0.5, linear_predict)))
    print("Percent Correct (when rated) within Range %f: %f" % (0.5, percent_correct_scored(xrating_test_y, 0.5, linear_predict)))

    xrating_randomforest = make_model(xrating_train_x, xrating_train_y, RandomForestRegressor())
    print("Stats for Random Forest Model")
    rf_predict = xrating_randomforest.predict(xrating_test_x)
    print("Predicted xrating on Test Data: %s" % rf_predict)

    rem_neg(rf_predict)

    rf_sse = sse(xrating_test_y, rf_predict)
    print("Max xRating: %s" % max(rf_predict))
    print("Sum SSE on Test Data: %f" % rf_sse)
    print("Avg SSE on Test Data: %f" % (rf_sse / len(rf_predict)))
    print("Coef of Determination on Test Data: %f" % (coef_det(xrating_test_x, xrating_test_y, xrating_randomforest)))
    print("Percent Correct within Range %f: %f" % (0.5, percent_correct(xrating_test_y, 0.5, rf_predict)))
    print("Percent Correct (when rated) within Range %f: %f" % (0.5, percent_correct_scored(xrating_test_y, 0.5, rf_predict)))

    dump(xrating_randomforest, 'xrating_model.joblib')

    # get importance
    importance = xrating_randomforest.feature_importances_
    # summarize feature importance
    for i,v in enumerate(importance):
     print('Feature: %s, Score: %.5f' % (xrating_data.columns.values[i],v))
    # plot feature importance
    pyplot.bar([x for x in range(len(importance))], importance)
    pyplot.show()

if __name__ == "__main__": main()
