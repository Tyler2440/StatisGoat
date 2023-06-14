import numpy as np
np.set_printoptions(threshold=20)
import pandas as pd
from sklearn.ensemble import RandomForestRegressor
from sklearn.linear_model import LinearRegression, LogisticRegression
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

def make_classification(train_y):
    y_list = []
    train_y_list = train_y
    for y in train_y_list:
        if y >= 1:
            y_val = 1
            y_list.append(y_val)
        else:
            y_val = 0
            y_list.append(y_val)
    return y_list

def main():
    col_names = ["avgSaved","avgBlocks","avgPenalties_saved","avgPenalties_conceded","avgMinutes","avgTeamconceded",
                "avgTeamPossesion","AvgOppshots","AvgOppshots_OG","AvgOppscored","height","width","player_saves"]
    xsaves_data = pd.read_csv("xsaves_train.csv", names=col_names, header=None)

    Highest_Assists = xsaves_data['player_saves'].max()
    # Spliting the model into 80% training and 20% test
    train_rows = int(xsaves_data.shape[0] * 0.8)
    xsaves_train = xsaves_data.iloc[:train_rows,:]
    xsaves_test = xsaves_data.iloc[train_rows:xsaves_data.shape[0],:]
    xsaves_train_x = xsaves_train.iloc[:,:-1]
    xsaves_train_y = xsaves_train.iloc[:,-1]
    xsaves_test_x = xsaves_test.iloc[:,:-1]
    xsaves_test_y = xsaves_test.iloc[:,-1]
    xsaves_train_y_classified = make_classification(xsaves_train_y)

    print("Models trained on features set: %s" % xsaves_data.columns.values)

    xsaves_linear = make_model(xsaves_train_x, xsaves_train_y, LinearRegression())
    print("Stats for Linear Regression Model")
    linear_predict = xsaves_linear.predict(xsaves_test_x)
    # print("Predicted xshots on Test Data: %s" % linear_predict)

    rem_neg(linear_predict)

    linear_sse = sse(xsaves_test_y, linear_predict)
    print("Sum SSE on Test Data: %f" % linear_sse)
    print("Avg SSE on Test Data: %f" % (linear_sse / len(linear_predict)))
    print("Coef of Determination on Test Data: %f" % (coef_det(xsaves_test_x, xsaves_test_y, xsaves_linear)))
    print("Percent Correct within Range %f: %f" % (0.1, percent_correct(xsaves_test_y, 0.1, linear_predict)))
    print("Percent Correct (Above 0) within Range %f: %f" % (0.5, percent_correct_scored(xsaves_test_y, 0.5, linear_predict)))

    xsaves_randomforest = make_model(xsaves_train_x, xsaves_train_y, RandomForestRegressor())
    print("Stats for Random Forest Model")
    rf_predict = xsaves_randomforest.predict(xsaves_test_x)
    # print("Predicted xshots on Test Data: %s" % rf_predict)

    rem_neg(rf_predict)

    rf_sse = sse(xsaves_test_y, rf_predict)
    print("Sum SSE on Test Data: %f" % rf_sse)
    print("Avg SSE on Test Data: %f" % (rf_sse / len(rf_predict)))
    print("Coef of Determination on Test Data: %f" % (coef_det(xsaves_test_x, xsaves_test_y, xsaves_randomforest)))
    print("Percent Correct within Range %f: %f" % (0.1, percent_correct(xsaves_test_y, 0.1, rf_predict)))
    print("Percent Correct (Above 0) within Range %f: %f" % (0.5, percent_correct_scored(xsaves_test_y, 0.5, rf_predict)))

    xsaves_logistic = make_model(xsaves_train_x, xsaves_train_y_classified, LogisticRegression(max_iter = 100000))
    print("Stats for Logistic Regression Model")
    logistic_predict_probability = xsaves_logistic.predict_proba(xsaves_test_x)
    logistic_predict_result = logistic_predict_probability[:,1]
    logistic_predict = logistic_predict_result * Highest_Assists

    logistic_sse = sse(xsaves_test_y, logistic_predict)
    print("Sum SSE on Test Data: %f" % logistic_sse)
    print("Avg SSE on Test Data: %f" % (logistic_sse / len(logistic_predict)))
    print("Coef of Determination on Test Data: %f" % (coef_det(xsaves_test_x, xsaves_test_y, xsaves_logistic)))
    print("Percent Correct within Range %f: %f" % (0.1, percent_correct(xsaves_test_y, 0.1, logistic_predict)))
    print("Percent Correct (Above 0) within Range %f: %f" % (0.5, percent_correct_scored(xsaves_test_y, 0.5, logistic_predict)))
    print("Percent Correct within Range %f: %f" % (0.5, percent_correct(xsaves_test_y, 0.5, logistic_predict)))
    print("Percent Correct (Above 0) within Range %f: %f" % (0.7, percent_correct_scored(xsaves_test_y, 0.7, logistic_predict)))

    dump(xsaves_randomforest, 'xsaves_model.joblib')

    # # get importance
    # importance = xshots_randomforest.feature_importances_
    # # summarize feature importance
    # for i,v in enumerate(importance):
    #  print('Feature: %s, Score: %.5f' % (col_names[i],v))
    # # plot feature importance
    # pyplot.bar([x for x in range(len(importance))], importance)
    # pyplot.show()

if __name__ == "__main__": main()