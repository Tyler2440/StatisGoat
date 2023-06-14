import numpy as np
np.set_printoptions(threshold=20)
import pandas as pd
from sklearn.neural_network import MLPClassifier
from sklearn.ensemble import RandomForestRegressor, RandomForestClassifier
from sklearn.linear_model import LinearRegression, LogisticRegression, Ridge
from sklearn.preprocessing import PolynomialFeatures
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
    col_names = ["avgGoals","psctScored","avgShots","avgShotsOnGoal","avgMinutes","avgDribbles","avgDribblesWon","avgTeamScores","avgOppConceded","height","width","player_goals"]
    xGoals_data = pd.read_csv("xg_train_positions.csv", names=col_names, header=None)
    #xGoals_data.drop(["avgDribbles","avgDribblesWon", "psctScored"], axis = 1, inplace = True)

    Highest_Goals = max(xGoals_data["player_goals"])
    train_rows = int(xGoals_data.shape[0] * 0.8)
    xGoals_train = xGoals_data.iloc[:train_rows,:]
    xGoals_test = xGoals_data.iloc[train_rows:xGoals_data.shape[0],:]

    # xGoals_train_0 = xGoals_train[xGoals_train['player_goals'] == 0] 
    # xGoals_train_scored = xGoals_train[xGoals_train['player_goals'] > 0]
    # xGoals_train_0 = xGoals_train_0.head(12500)
    # xGoals_train = pd.concat([xGoals_train_0, xGoals_train_scored])

    xGoals_train_x = xGoals_train.iloc[:,:-1]
    xGoals_train_y = xGoals_train.iloc[:,-1]
    xGoals_test_x = xGoals_test.iloc[:,:-1]
    xGoals_test_y = xGoals_test.iloc[:,-1]
    xGoals_train_y_classified = make_classification(xGoals_train_y)
    
    print("Models trained on features set: %s" % xGoals_data.columns.values)
    print("<------------------------------------------------------------------>")

    xg_linear = make_model(xGoals_train_x, xGoals_train_y, LinearRegression())
    print("Stats for Linear Regression Model")

    linear_predict = xg_linear.predict(xGoals_test_x)
    rem_neg(linear_predict)

    linear_sse = sse(xGoals_test_y, linear_predict)
    print("Sum SSE on Test Data: %f" % linear_sse)
    print("Avg SSE on Test Data: %f" % (linear_sse / len(linear_predict)))
    print("Max xG: %f" % max(linear_predict))
    pyplot.plot([i/10.0 for i in range(1, 11)],
                [percent_correct(xGoals_test_y, i/10.0, linear_predict) for i in range(1, 11)], 
                color="red", label="linear regression")
    pyplot.plot([i/10.0 for i in range(1, 11)],
                [percent_correct_scored(xGoals_test_y, i/10.0, linear_predict) for i in range(1, 11)],
                color="red")
    # print("Coef of Determination on Test Data: %f" % (coef_det(xGoals_test_x, xGoals_test_y, xg_linear)))
    # print("Percent Correct within Range %f: %f" % (0.1, percent_correct(xGoals_test_y, 0.1, linear_predict)))
    # print("Percent Correct (When Scored) within Range %f: %f" % (0.5, percent_correct_scored(xGoals_test_y, 0.5, linear_predict)))
    print("<------------------------------------------------------------------>")

    poly = PolynomialFeatures(degree=3, include_bias=False)
    xg_train_poly = poly.fit_transform(xGoals_train_x)
    xg_test_poly = poly.fit_transform(xGoals_test_x)

    xg_poly = make_model(xg_train_poly, xGoals_train_y, Ridge(alpha=0.01))
    print("Stats for Poly Regression Model")

    poly_predict = xg_poly.predict(xg_test_poly)
    rem_neg(poly_predict)

    poly_sse = sse(xGoals_test_y, poly_predict)
    print("Sum SSE on Test Data: %f" % poly_sse)
    print("Avg SSE on Test Data: %f" % (poly_sse / len(poly_predict)))
    print("Max xG: %f" % max(poly_predict))
    pyplot.plot([i/10.0 for i in range(1, 11)],
                [percent_correct(xGoals_test_y, i/10.0, poly_predict) for i in range(1, 11)], 
                color="purple", label="poly regression")
    pyplot.plot([i/10.0 for i in range(1, 11)],
                [percent_correct_scored(xGoals_test_y, i/10.0, poly_predict) for i in range(1, 11)],
                color="purple")
    #print("Coef of Determination on Test Data: %f" % (coef_det(xGoals_test_x, xGoals_test_y, xg_linear)))
    #print("Percent Correct within Range %f: %f" % (0.1, percent_correct(xGoals_test_y, 0.1, linear_predict)))
    #print("Percent Correct (When Scored) within Range %f: %f" % (0.5, percent_correct_scored(xGoals_test_y, 0.5, linear_predict)))
    print("<------------------------------------------------------------------>")

    xg_randomforest = make_model(xGoals_train_x, xGoals_train_y, RandomForestRegressor())
    print("Stats for Random Forrest Model")

    rf_predict = xg_randomforest.predict(xGoals_test_x)
    rem_neg(rf_predict)

    rf_sse = sse(xGoals_test_y, rf_predict)
    print("Sum SSE on Test Data: %f" % rf_sse)
    print("Avg SSE on Test Data: %f" % (rf_sse / len(rf_predict)))
    print("Max xG: %f" % max(rf_predict))
    pyplot.plot([i/10.0 for i in range(1, 11)],
                [percent_correct(xGoals_test_y, i/10.0, rf_predict) for i in range(1, 11)],
                color="blue", label="rf regression")
    pyplot.plot([i/10.0 for i in range(1, 11)],
                [percent_correct_scored(xGoals_test_y, i/10.0, rf_predict) for i in range(1, 11)],
                color="blue")
    # print("Coef of Determination on Test Data: %f" % (coef_det(xGoals_test_x, xGoals_test_y, xg_randomforest)))
    # print("Percent Correct within Range %f: %f" % (0.1, percent_correct(xGoals_test_y, 0.1, rf_predict)))
    # print("Percent Correct (When Scored) within Range %f: %f" % (0.5, percent_correct_scored(xGoals_test_y, 0.5, rf_predict)))
    print("<------------------------------------------------------------------>")

    xGoals_logistic = make_model(xGoals_train_x, xGoals_train_y_classified, LogisticRegression(max_iter = 100000))
    print("Stats for Logistic Regression Model")
    logistic_predict_probability = xGoals_logistic.predict_proba(xGoals_test_x)
    logistic_predict_result = logistic_predict_probability[:,1]
    logistic_predict = logistic_predict_result * (Highest_Goals)

    logistic_sse = sse(xGoals_test_y, logistic_predict)
    print("Sum SSE on Test Data: %f" % logistic_sse)
    print("Avg SSE on Test Data: %f" % (logistic_sse / len(logistic_predict)))
    print("Max xG: %f" % max(logistic_predict))
    pyplot.plot([i/10.0 for i in range(1, 11)],
                [percent_correct(xGoals_test_y, i/10.0, logistic_predict) for i in range(1, 11)],
                color="green", label="logistic regression")
    pyplot.plot([i/10.0 for i in range(1, 11)],
                [percent_correct_scored(xGoals_test_y, i/10.0, logistic_predict) for i in range(1, 11)],
                color="green")
    # print("Coef of Determination on Test Data: %f" % (coef_det(xGoals_test_x, xGoals_test_y, xGoals_logistic)))
    # print("Percent Correct within Range %f: %f" % (0.1, percent_correct(xGoals_test_y, 0.1, logistic_predict)))
    # print("Percent Correct (Above 0) within Range %f: %f" % (0.5, percent_correct_scored(xGoals_test_y, 0.5, logistic_predict)))
    # print("Percent Correct within Range %f: %f" % (0.5, percent_correct(xGoals_test_y, 0.5, logistic_predict)))
    # print("Percent Correct (Above 0) within Range %f: %f" % (0.7, percent_correct_scored(xGoals_test_y, 0.7, logistic_predict)))
    print("<------------------------------------------------------------------>")

    # xGoals_NN = make_model(xGoals_train_x, xGoals_train_y_classified, MLPClassifier(hidden_layer_sizes=50,max_iter=100,random_state=1))
    # print("Stats for Neural Network Model")
    # NN_predict_probability = xGoals_NN.predict_proba(xGoals_test_x)
    # NN_predict_result = NN_predict_probability[:,1]
    # NN_predict = NN_predict_result * (Highest_Goals)

    # NN_sse = sse(xGoals_test_y, NN_predict)
    # print("Sum SSE on Test Data: %f" % NN_sse)
    # print("Avg SSE on Test Data: %f" % (NN_sse / len(NN_predict)))
    # print("Max xG: %f" % max(NN_predict))
    # pyplot.plot([i/10.0 for i in range(1, 11)],
    #             [percent_correct(xGoals_test_y, i/10.0, NN_predict) for i in range(1, 11)],
    #             color="orange", label="neural network")
    # pyplot.plot([i/10.0 for i in range(1, 11)],
    #             [percent_correct_scored(xGoals_test_y, i/10.0, NN_predict) for i in range(1, 11)],
    #             color="orange")
    # # print("Coef of Determination on Test Data: %f" % (coef_det(xGoals_test_x, xGoals_test_y, xGoals_NN)))
    # # print("Percent Correct within Range %f: %f" % (0.1, percent_correct(xGoals_test_y, 0.1, NN_predict)))
    # # print("Percent Correct (Above 0) within Range %f: %f" % (0.5, percent_correct_scored(xGoals_test_y, 0.5, NN_predict)))
    # # print("Percent Correct within Range %f: %f" % (0.5, percent_correct(xGoals_test_y, 0.5, NN_predict)))
    # # print("Percent Correct (Above 0) within Range %f: %f" % (0.7, percent_correct_scored(xGoals_test_y, 0.7, NN_predict)))
    # print("<------------------------------------------------------------------>")
    
    # xGoals_Forest_Classifier = make_model(xGoals_train_x, xGoals_train_y_classified, RandomForestClassifier(random_state = 0))
    # print("Stats for Random Forrest Classifier Model")
    # RFC_predict_probability = xGoals_Forest_Classifier.predict_proba(xGoals_test_x)
    # RFC_predict_result = RFC_predict_probability[:,1]
    # RFC_predict = RFC_predict_result * (Highest_Goals)

    # RFC_sse = sse(xGoals_test_y, RFC_predict)
    # print("Sum SSE on Test Data: %f" % RFC_sse)
    # print("Avg SSE on Test Data: %f" % (RFC_sse / len(RFC_predict)))
    # print("Max xG: %f" % max(RFC_predict))
    # pyplot.plot([i/10.0 for i in range(1, 11)],
    #             [percent_correct(xGoals_test_y, i/10.0, RFC_predict) for i in range(1, 11)],
    #             color="brown", label="rf classifier")
    # pyplot.plot([i/10.0 for i in range(1, 11)],
    #             [percent_correct_scored(xGoals_test_y, i/10.0, RFC_predict) for i in range(1, 11)],
    #             color="brown")
    # # print("Coef of Determination on Test Data: %f" % (coef_det(xGoals_test_x, xGoals_test_y, xGoals_Forest_Classifier)))
    # # print("Percent Correct within Range %f: %f" % (0.1, percent_correct(xGoals_test_y, 0.1, RFC_predict)))
    # # print("Percent Correct (Above 0) within Range %f: %f" % (0.5, percent_correct_scored(xGoals_test_y, 0.5, RFC_predict)))
    # # print("Percent Correct within Range %f: %f" % (0.5, percent_correct(xGoals_test_y, 0.5, RFC_predict)))
    # # print("Percent Correct (Above 0) within Range %f: %f" % (0.7, percent_correct_scored(xGoals_test_y, 0.7, RFC_predict)))
    # print("<------------------------------------------------------------------>")
    
    dump(xg_poly, 'xg_model.joblib')

    pyplot.title("Model Accuracies")
    pyplot.xlabel("Error Range")
    pyplot.ylabel("Accuracy")
    pyplot.legend()
    pyplot.savefig("accuracies.png")
    pyplot.show()

    # # get importance
    # importance = xg_randomforest.feature_importances_
    # # summarize feature importance
    # for i,v in enumerate(importance):
    #  print('Feature: %s, Score: %.5f' % (col_names[i],v))
    # # plot feature importance
    # pyplot.bar([x for x in range(len(importance))], importance)
    # pyplot.show()

if __name__ == "__main__": main()
