import numpy as np
np.set_printoptions(threshold=20)
import pandas as pd
from sklearn.neural_network import MLPClassifier
from sklearn.ensemble import RandomForestRegressor, RandomForestClassifier
from sklearn.linear_model import LinearRegression, LogisticRegression
from matplotlib import pyplot
from joblib import dump, load
import csv

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
    col_names = ["avgdribbles", "avgminutes", "avggoals", "avgassists", "avgtackles", "avgpossession", "grid0", "grid1", "dribbles"]
    xdribbles_data = pd.read_csv("xdribbles.csv", names=col_names, header=None)
    #xdribbles_data.drop(["avgdribbles"], axis = 1, inplace = True)

    Highest_dribbles = xdribbles_data['dribbles'].max()
    # Spliting the model into 80% training and 20% test
    train_rows = int(xdribbles_data.shape[0] * 0.8)
    xdribbles_train = xdribbles_data.iloc[:train_rows,:]
    xdribbles_test = xdribbles_data.iloc[train_rows:xdribbles_data.shape[0],:]
    xdribbles_train_x = xdribbles_train.iloc[:,:-1]
    xdribbles_train_y = xdribbles_train.iloc[:,-1]
    xdribbles_test_x = xdribbles_test.iloc[:,:-1]
    xdribbles_test_y = xdribbles_test.iloc[:,-1]
    xdribbles_train_y_classified = make_classification(xdribbles_train_y)

    print("Models trained on features set: %s" % xdribbles_data.columns.values)
    print("<------------------------------------------------------------------>")

    xdribbles_linear = make_model(xdribbles_train_x, xdribbles_train_y, LinearRegression())
    print("Stats for Linear Regression Model")
    linear_predict = xdribbles_linear.predict(xdribbles_test_x)

    rem_neg(linear_predict)

    linear_sse = sse(xdribbles_test_y, linear_predict)
    print("Sum SSE on Test Data: %f" % linear_sse)
    print("Avg SSE on Test Data: %f" % (linear_sse / len(linear_predict)))
    print("Max xDribbles: %f" % max(linear_predict))
    pyplot.plot([i/10.0 for i in range(1, 11)],
                [percent_correct(xdribbles_test_y, i/10.0, linear_predict) for i in range(1, 11)], 
                color="purple", label="linear regression")
    pyplot.plot([i/10.0 for i in range(1, 11)],
                [percent_correct_not_zero(xdribbles_test_y, i/10.0, linear_predict) for i in range(1, 11)],
                color="purple")
    # print("Coef of Determination on Test Data: %f" % (coef_det(xdribbles_test_x, xdribbles_test_y, xdribbles_linear)))
    # print("Percent Correct within Range %f: %f" % (0.1, percent_correct(xdribbles_test_y, 0.1, linear_predict)))
    # print("Percent Correct (not 0) within Range %f: %f" % (0.1, percent_correct_not_zero(xdribbles_test_y, 0.1, linear_predict)))
    # print("Percent Correct within Range %f: %f" % (0.5, percent_correct(xdribbles_test_y, 0.5, linear_predict)))
    # print("Percent Correct (not 0) within Range %f: %f" % (0.5, percent_correct_not_zero(xdribbles_test_y, 0.5, linear_predict)))
    print("<------------------------------------------------------------------>")

    xdribbles_randomforest = make_model(xdribbles_train_x, xdribbles_train_y, RandomForestRegressor())
    print("Stats for Random Forest Model")
    rf_predict = xdribbles_randomforest.predict(xdribbles_test_x)

    rem_neg(rf_predict)

    rf_sse = sse(xdribbles_test_y, rf_predict)
    print("Sum SSE on Test Data: %f" % rf_sse)
    print("Avg SSE on Test Data: %f" % (rf_sse / len(rf_predict)))
    print("Max xDribbles: %f" % max(rf_predict))
    pyplot.plot([i/10.0 for i in range(1, 11)],
                [percent_correct(xdribbles_test_y, i/10.0, rf_predict) for i in range(1, 11)], 
                color="green", label="rf regression")
    pyplot.plot([i/10.0 for i in range(1, 11)],
                [percent_correct_not_zero(xdribbles_test_y, i/10.0, rf_predict) for i in range(1, 11)],
                color="green")
    # print("Coef of Determination on Test Data: %f" % (coef_det(xdribbles_test_x, xdribbles_test_y, xdribbles_randomforest)))
    # print("Percent Correct within Range %f: %f" % (0.1, percent_correct(xdribbles_test_y, 0.1, rf_predict)))
    # print("Percent Correct (not 0) within Range %f: %f" % (0.1, percent_correct_not_zero(xdribbles_test_y, 0.1, rf_predict)))
    # print("Percent Correct within Range %f: %f" % (0.5, percent_correct(xdribbles_test_y, 0.5, rf_predict)))
    # print("Percent Correct (not 0) within Range %f: %f" % (0.5, percent_correct_not_zero(xdribbles_test_y, 0.5, rf_predict)))
    print("<------------------------------------------------------------------>")

    xdribbles_logistic = make_model(xdribbles_train_x, xdribbles_train_y_classified, LogisticRegression(max_iter = 100000))
    print("Stats for Logistic Regression Model")
    logistic_predict_probability = xdribbles_logistic.predict_proba(xdribbles_test_x)
    logistic_predict_result = logistic_predict_probability[:,1]
    logistic_predict = logistic_predict_result * (Highest_dribbles)

    logistic_sse = sse(xdribbles_test_y, logistic_predict)
    print("Sum SSE on Test Data: %f" % logistic_sse)
    print("Avg SSE on Test Data: %f" % (logistic_sse / len(logistic_predict)))
    print("Max xDribbles: %f" % max(logistic_predict))
    pyplot.plot([i/10.0 for i in range(1, 11)],
                [percent_correct(xdribbles_test_y, i/10.0, logistic_predict) for i in range(1, 11)], 
                color="yellow", label="logistic regression")
    pyplot.plot([i/10.0 for i in range(1, 11)],
                [percent_correct_not_zero(xdribbles_test_y, i/10.0, logistic_predict) for i in range(1, 11)],
                color="yellow")
    # print("Coef of Determination on Test Data: %f" % (coef_det(xdribbles_test_x, xdribbles_test_y, xdribbles_logistic)))
    # print("Percent Correct within Range %f: %f" % (0.1, percent_correct(xdribbles_test_y, 0.1, logistic_predict)))
    # print("Percent Correct (Above 0) within Range %f: %f" % (0.5, percent_correct_not_zero(xdribbles_test_y, 0.5, logistic_predict)))
    # print("Percent Correct within Range %f: %f" % (0.5, percent_correct(xdribbles_test_y, 0.5, logistic_predict)))
    # print("Percent Correct (Above 0) within Range %f: %f" % (0.7, percent_correct_not_zero(xdribbles_test_y, 0.7, logistic_predict)))
    print("<------------------------------------------------------------------>")

    xdribbles_NN = make_model(xdribbles_train_x, xdribbles_train_y_classified, MLPClassifier(hidden_layer_sizes=50,max_iter=100,random_state=1))
    print("Stats for Neural Network Model")
    NN_predict_probability = xdribbles_NN.predict_proba(xdribbles_test_x)
    NN_predict_result = NN_predict_probability[:,1]
    NN_predict = NN_predict_result * (Highest_dribbles)

    NN_sse = sse(xdribbles_test_y, NN_predict)
    print("Sum SSE on Test Data: %f" % NN_sse)
    print("Avg SSE on Test Data: %f" % (NN_sse / len(NN_predict)))
    print("Max xDribbles: %f" % max(NN_predict))
    pyplot.plot([i/10.0 for i in range(1, 11)],
                [percent_correct(xdribbles_test_y, i/10.0, NN_predict) for i in range(1, 11)], 
                color="green", label="neural network")
    pyplot.plot([i/10.0 for i in range(1, 11)],
                [percent_correct_not_zero(xdribbles_test_y, i/10.0, NN_predict) for i in range(1, 11)],
                color="green")
    # print("Coef of Determination on Test Data: %f" % (coef_det(xdribbles_test_x, xdribbles_test_y, xdribbles_NN)))
    # print("Percent Correct within Range %f: %f" % (0.1, percent_correct(xdribbles_test_y, 0.1, NN_predict)))
    # print("Percent Correct (Above 0) within Range %f: %f" % (0.5, percent_correct_not_zero(xdribbles_test_y, 0.5, NN_predict)))
    # print("Percent Correct within Range %f: %f" % (0.5, percent_correct(xdribbles_test_y, 0.5, NN_predict)))
    # print("Percent Correct (Above 0) within Range %f: %f" % (0.7, percent_correct_not_zero(xdribbles_test_y, 0.7, NN_predict)))
    print("<------------------------------------------------------------------>")
    
    xdribbles_Forest_Classifer = make_model(xdribbles_train_x, xdribbles_train_y_classified, RandomForestClassifier(random_state = 0))
    print("Stats for Random Forest Classifier Model")
    RFC_predict_probability = xdribbles_Forest_Classifer.predict_proba(xdribbles_test_x)
    RFC_predict_result = RFC_predict_probability[:,1]
    RFC_predict = RFC_predict_result * (Highest_dribbles)

    RFC_sse = sse(xdribbles_test_y, RFC_predict)
    print("Sum SSE on Test Data: %f" % RFC_sse)
    print("Avg SSE on Test Data: %f" % (RFC_sse / len(RFC_predict)))
    print("Max xDribbles: %f" % max(RFC_predict))
    pyplot.plot([i/10.0 for i in range(1, 11)],
                [percent_correct(xdribbles_test_y, i/10.0, RFC_predict) for i in range(1, 11)], 
                color="brown", label="rf classifier")
    pyplot.plot([i/10.0 for i in range(1, 11)],
                [percent_correct_not_zero(xdribbles_test_y, i/10.0, RFC_predict) for i in range(1, 11)],
                color="brown")
    # print("Coef of Determination on Test Data: %f" % (coef_det(xdribbles_test_x, xdribbles_test_y, xdribbles_Forest_Classifer)))
    # print("Percent Correct within Range %f: %f" % (0.1, percent_correct(xdribbles_test_y, 0.1, RFC_predict)))
    # print("Percent Correct (Above 0) within Range %f: %f" % (0.5, percent_correct_not_zero(xdribbles_test_y, 0.5, RFC_predict)))
    # print("Percent Correct within Range %f: %f" % (0.5, percent_correct(xdribbles_test_y, 0.5, RFC_predict)))
    # print("Percent Correct (Above 0) within Range %f: %f" % (0.7, percent_correct_not_zero(xdribbles_test_y, 0.7, RFC_predict)))
    print("<------------------------------------------------------------------>")

    pyplot.title("Model Accuracies")
    pyplot.xlabel("Error Range")
    pyplot.ylabel("Accuracy")
    pyplot.legend()
    pyplot.savefig("accuracies.png")
    pyplot.show()
    
    # print(Highest_dribbles)
    dump(xdribbles_linear, 'xdribbles_model.joblib')

    # # get importance
    # importance = xg_randomforest.feature_importances_
    # # summarize feature importance
    # for i,v in enumerate(importance):
    #  print('Feature: %s, Score: %.5f' % (col_names[i],v))
    # # plot feature importance
    # pyplot.bar([x for x in range(len(importance))], importance)
    # pyplot.show()

if __name__ == "__main__": main()
