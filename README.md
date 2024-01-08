# Epam Test Task

## Sql Challenge
[Sql challenge solution with comments](./school-user-query.sql)

## Execution Steps

Launch solution using Visual Studio 2022.
Tests can be executed through the `Test Explorer` window.

## Acceptance Criteria

1. Users are able to create only one Study Group for a single Subject  
    a. Users can provide a name for the Study Group with size between 5-30 characters  
    b. The only valid Subjects are: Math, Chemistry, Physics  
    c. We want to record when Study Groups were created  
2. Users can join Study Groups for different Subjects  
3. Users can check the list of all existing Study Groups  
    a. Users can also filter Study Groups by a given Subject  
    b. Users can sort to see most recently created Study Groups or oldest ones  
4. Users can leave Study Groups they joined  

## Architecture and Assumptions

Both the backend and the test projects are written using C#.
The system is composed of a simple Web Service and Database.
The web UI utilizes REST APIs to communicate with the backend.
For this implementation no authenticatio method is used, it is assumed that authentication is out of scope.
The database utilized is relational, and is implemented as an in-memory database for testing purposes, but in production could be easily switched to any relational database such as PostgreSQL, MySQL, etc.
The entire database configuration is code-driven by utilizing the .NET 8 Entity Framework.
Study groups contain an owner, who is the first user in the users list. When the owner leaves the study group they own, the group is deleted. This also ensures there are no empty study groups.

Testing is written in a different project, and organized by type, to keep the project simpler.
The implemented test strategies are unit, integration and end to end.

The web UI allows the user to access the entire functionality defined by the acceptance criteria via some kind of panel.

## Test Cases

### Unit Tests

Details:
> Uses mock IRepository  
> Instantiates the controller directly  
> Only tests rules implemented on the controller layer
> Some functionality such as owner destroying group is implemented within the repository itself, so the mock repository has to also support it

Tests:

* Validate valid study group name sizes are accepted
* Validate invalid study group name sizes are rejected
* Validate that the only valid subjects are Math, Chemistry and Physics
* Validate that only existing users can be assigned to study groups
* Validate that creating study group creates the study group correctly
* Validate that an user can only create one study group for each subject
* Validate that study group can only be created if all fields are present
* Validate that joining and leaving a study group multiple times work correctly
* Validate that an user can join multiple study groups
* Validate that getting study group returns the existing study groups
* Validate that the owner leaving a study group destroys the study group
* Validate that searching study group only returns study groups with the given subject
* Validate that getting study group sorted by date returns the groups sorted by earliest or oldest

### Integration Tests

> Uses in memory database  
> Leverages .NET web testing infrastructure and HttpClient  
> Validates actual database persistency and relationships
> Is executed as a single long test

Test steps:

* Register users
* Verify that users are created correctly
* Create study groups
* Verify that study groups are created correctly
* Join groups as users
* Verify that joining groups works correctly
* Leave a group as user
* Verify that leaving groups works correctly
* Leave a group as owner
* Verify that group is destroyed
* Filter groups by subject
* Verify that groups are correctly filtered by subject

#### Exception scenarios

In integration tests there are multiple exception scenarios to be tested.
These exceptions were not implemented for brevity, or this challenge would take too long to complete.
They include:

* Verify that a study group cannot be created if the owner id does not exist
* Verify that a study group is destroyed when the owner is deleted
* Verify the correct errors when calling apis with ids for resources which do not exist
* Verify that the requests throw errors when the database is not connected
* Verify that a study group can never be empty, as it has to have at least the owner within the list of users
* Verify that a user cannot join a study group twice

And so on.

### End to end Tests

The end to end tests will require the system infrastructure to be set up before test execution.
For API tests there is no need for manual tests.
For web, functional tests can be easily automated, whereas usability and accessibility tests are easier to execute manually.

Web tests:

* Validate at least one type of each user interaction
* Validate that an user can only access and modify their own data
* Validate that the UI properly updates and displays the relevant information
* Validate that the UI ensures that the user inputs required fields before making a request, and that clear error messages are displayed
* Validate the usability and accessibility aspects of the system
* Validate loading times and UI performance
* Validate the interface layout
* Validate the responsive elements, images and fields
