# Import the function to test
from script import task
import pytest

# Test function
def test_task():
    result = task()
    expected = "Hello, world!"
    assert result == expected, f"Expected {expected}, but got {result}"

# No need for a main function, pytest will discover and run this test automatically
