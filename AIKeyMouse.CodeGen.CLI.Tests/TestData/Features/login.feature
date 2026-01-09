Feature: User Login
    As a user
    I want to login to the application
    So that I can access my account

    Scenario: Successful login
        Given I am on the login page
        When I enter username "testuser"
        And I enter password "password123"
        And I click the login button
        Then I should be on the dashboard page
        And I should see a welcome message

    Scenario: Failed login with invalid credentials
        Given I am on the login page
        When I enter username "invalid"
        And I enter password "wrong"
        And I click the login button
        Then I should see an error message "Invalid credentials"
        And I should remain on the login page
