from sklearn.tree import DecisionTreeClassifier


def _trainModel(X_train, y_train):
    model = DecisionTreeClassifier()
    model.fit(X_train, y_train)



    return model

def trainModel(X_train, y_train):
    return _trainModel(X_train, y_train)
