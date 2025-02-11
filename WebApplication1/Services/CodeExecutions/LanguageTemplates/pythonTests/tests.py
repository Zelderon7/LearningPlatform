#By default this file will not be visible to anyone solving this task
#You can change this by modifying the 'visible' variable below

#Do not delete this import
from script import task

# Write your tests below :)

def main():
	# Calling the test function
    test_task()
    print("Test passed!")

def test_task():
	assert task() == 'Hello, world!'


# Ensure the main function is called when the script is run
if __name__ == "__main__":
    main()