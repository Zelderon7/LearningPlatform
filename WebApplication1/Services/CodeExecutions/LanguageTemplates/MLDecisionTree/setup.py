import pandas as pd
from sklearn.preprocessing import LabelEncoder
from sklearn.model_selection import train_test_split

# Global variables
_X_train = None
_X_test = None
_y_train = None
_y_test = None

def runSetup():
    createCSV()
    prepareData()

def createCSV():
    data = {
        'Hero1': ['Ahri', 'Lux', 'Jinx', 'Katarina', 'MissFortune', 'Darius', 'Leona', 'Draven', 'Annie', 'Ashe'],
        'Hero2': ['Yasuo', 'Thresh', 'Ezreal', 'Zed', 'Sivir', 'Garen', 'Morgana', 'Caitlyn', 'Malzahar', 'Tristana'],
        'Hero1_Stats': [85, 75, 95, 88, 82, 95, 80, 90, 78, 84],
        'Hero2_Stats': [90, 70, 85, 92, 80, 93, 85, 88, 82, 79],
        'Winner': [2, 1, 1, 2, 1, 1, 2, 1, 2, 1]
    }

    df = pd.DataFrame(data)
    df.to_csv('lol_battle_data.csv', index=False)
    print('CSV file created successfully!')

def prepareData():
    global _X_train, _X_test, _y_train, _y_test  # Use global variables

    df = pd.read_csv('lol_battle_data.csv')

    le = LabelEncoder()
    df['Hero1'] = le.fit_transform(df['Hero1'])
    df['Hero2'] = le.fit_transform(df['Hero2'])

    X = df[['Hero1', 'Hero2', 'Hero1_Stats', 'Hero2_Stats']]
    y = df['Winner']

    _X_train, _X_test, _y_train, _y_test = train_test_split(X, y, test_size=0.2, random_state=42)
    
    print('Data prepared successfully!')

def getTestX():
    if _X_test is None:
        raise ValueError("Error: X_test is not initialized. Call prepareData() first.")
    return _X_test

def getTestY():
    if _y_test is None:
        raise ValueError("Error: y_test is not initialized. Call prepareData() first.")
    return _y_test

def getTrainX():
    if _X_train is None:
        raise ValueError("Error: X_train is not initialized. Call prepareData() first.")
    return _X_train

def getTrainY():
    if _y_train is None:
        raise ValueError("Error: y_train is not initialized. Call prepareData() first.")
    return _y_train
