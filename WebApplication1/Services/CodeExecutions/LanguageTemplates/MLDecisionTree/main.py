from setup import runSetup, getTestX, getTestY, getTrainX, getTrainY
from modelTraining import trainModel
from sklearn.metrics import accuracy_score

# Set up the data
runSetup()

#Get the training data
X_train = getTrainX()
y_train = getTrainY()

# Get the test data
X_test = getTestX()
y_test = getTestY()

# Train the model
model = trainModel(X_train, y_train)

# Make predictions and check accuracy
y_pred = model.predict(X_test)

accuracy = accuracy_score(y_test, y_pred)

print(f'Model Accuracy: {accuracy:.2f}')
