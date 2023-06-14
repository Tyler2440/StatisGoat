import numpy as np
np.set_printoptions(threshold=20)
import pandas as pd
from matplotlib import pyplot
from sklearn.ensemble import RandomForestRegressor
from sklearn.linear_model import LinearRegression
from joblib import dump

def make_model(train_x, train_y, model): return model.fit(train_x, train_y)
def rem_neg(predicts): predicts[predicts < 0] = 0
def sse(test_y, predicts): return sum([(test_y.tolist()[i] - predicts[i])**2  for i in range(len(predicts))])
def coef_det(test_x, test_y, model): return model.score(test_x, test_y)
def percent_correct(test_y, tol, predicts): return sum([1 if abs(test_y.tolist()[i] - predicts[i]) <= tol else 0 for i in range(len(predicts))]) / len(predicts)

def percent_correct_not_zero(test_y, tol, predicts):
    scored = 0
    correct = 0
    for i in range(len(test_y.tolist())):
        if test_y.tolist()[i] < 1: continue
        scored += 1
        if abs(test_y.tolist()[i] - predicts[i]) <= tol: correct += 1
    return correct / scored

def main():
    col_names = ["avgoppconceded", "avgminutes", "avgpasses", "avgtackles", "avgblocks", "avginterceptions", "avgduels", "avgduels_won", "avgfouls_committed", "avgyellow", "avgred", "avgpossession", "grid0", "grid1", "tackles", "interceptions"]
    xt_data = pd.read_csv("xtack_xint.csv", names=col_names, header=None)
    xt_data.drop(["interceptions"], axis = 1, inplace = True)

    # Spliting the model into 80% training and 20% test
    train_rows = int(xt_data.shape[0] * 0.8)
    xt_train = xt_data.iloc[:train_rows,:]
    xt_test = xt_data.iloc[train_rows:xt_data.shape[0],:]
    xt_train_x = xt_train.iloc[:,:-1]
    xt_train_y = xt_train.iloc[:,-1]
    xt_test_x = xt_test.iloc[:,:-1]
    xt_test_y = xt_test.iloc[:,-1]

    print("Models trained on features set: %s" % xt_data.columns.values)
    print("<------------------------------------------------------------------>")

    xt_linear = make_model(xt_train_x, xt_train_y, LinearRegression())
    print("Stats for Linear Regression Model")
    linear_predict = xt_linear.predict(xt_test_x)

    rem_neg(linear_predict)

    linear_sse = sse(xt_test_y, linear_predict)
    print("Sum SSE on Test Data: %f" % linear_sse)
    print("Avg SSE on Test Data: %f" % (linear_sse / len(linear_predict)))
    print("Max xTackles: %f" % max(linear_predict))
    pyplot.plot([i/10.0 for i in range(1, 11)],
                [percent_correct(xt_test_y, i/10.0, linear_predict) for i in range(1, 11)], 
                color="purple", label="linear regression")
    pyplot.plot([i/10.0 for i in range(1, 11)],
                [percent_correct_not_zero(xt_test_y, i/10.0, linear_predict) for i in range(1, 11)],
                color="purple")
    # print("Coef of Determination on Test Data: %f" % (coef_det(xt_test_x, xt_test_y, xt_linear)))
    # print("Percent Correct within Range %f: %f" % (0.1, percent_correct(xt_test_y, 0.1, linear_predict)))
    # print("Percent Correct (not 0) within Range %f: %f" % (0.1, percent_correct_not_zero(xt_test_y, 0.1, linear_predict)))
    # print("Percent Correct within Range %f: %f" % (0.5, percent_correct(xt_test_y, 0.5, linear_predict)))
    # print("Percent Correct (not 0) within Range %f: %f" % (0.5, percent_correct_not_zero(xt_test_y, 0.5, linear_predict)))
    print("<------------------------------------------------------------------>")

    xt_randomforest = make_model(xt_train_x, xt_train_y, RandomForestRegressor())
    print("Stats for Random Forest Model")
    rf_predict = xt_randomforest.predict(xt_test_x)

    rem_neg(rf_predict)

    rf_sse = sse(xt_test_y, rf_predict)
    print("Sum SSE on Test Data: %f" % rf_sse)
    print("Avg SSE on Test Data: %f" % (rf_sse / len(rf_predict)))
    print("Max xTackles: %f" % max(rf_predict))
    pyplot.plot([i/10.0 for i in range(1, 11)],
                [percent_correct(xt_test_y, i/10.0, rf_predict) for i in range(1, 11)], 
                color="green", label="rf regression")
    pyplot.plot([i/10.0 for i in range(1, 11)],
                [percent_correct_not_zero(xt_test_y, i/10.0, rf_predict) for i in range(1, 11)],
                color="green")
    # print("Coef of Determination on Test Data: %f" % (coef_det(xt_test_x, xt_test_y, xt_randomforest)))
    # print("Percent Correct within Range %f: %f" % (0.1, percent_correct(xt_test_y, 0.1, rf_predict)))
    # print("Percent Correct (not 0) within Range %f: %f" % (0.1, percent_correct_not_zero(xt_test_y, 0.1, rf_predict)))
    # print("Percent Correct within Range %f: %f" % (0.5, percent_correct(xt_test_y, 0.5, rf_predict)))
    # print("Percent Correct (not 0) within Range %f: %f" % (0.5, percent_correct_not_zero(xt_test_y, 0.5, rf_predict)))
    print("<------------------------------------------------------------------>")

    pyplot.title("Model Accuracies")
    pyplot.xlabel("Error Range")
    pyplot.ylabel("Accuracy")
    pyplot.legend()
    pyplot.savefig("accuracies.png")
    pyplot.show()

    dump(xt_linear, 'xtack_model.joblib')

    # # get importance
    # importance = xg_randomforest.feature_importances_
    # # summarize feature importance
    # for i,v in enumerate(importance):
    #  print('Feature: %s, Score: %.5f' % (col_names[i],v))
    # # plot feature importance
    # pyplot.bar([x for x in range(len(importance))], importance)
    # pyplot.show()

if __name__ == "__main__": main()
