# FNDTAPI
Calendar and Task/TODO List REST API created for FDNT App using ASP.NET Core Web API.

## Used External Libraries
* [MongoDB Driver](https://docs.mongodb.com/drivers/csharp)

## Possible requests

### HTTP POST
#### 1) `/api/v1.0/TaskList/tasklists`
Adds sent instance of `TaskList` to database.

If request succeeded: Returns `Guid`, which is ID of a newly inserted to database `TaskList`

otherwise: Returns HTTP Response 422: Unprocessable Entity, when sent `TaskList` is null or it's properties are empty.

#### 2) `/api/v1.0/TaskList/tasklists/event`
Adds given `TaskList` to specific `CalendarEvent`. 

Important: Sent values has to be in format of `Dictionary<string, string>`, like this:
```json
{
	"eventID": "Guid of the CalendarEvent",
	"taskList": "Here you place serialized TaskList"
}
```

If request succeeded: Returns `Guid`, which is ID of a newly inserted to database `TaskList`, which is assigned to given `CalendarEvent`.

otherwise:

Returns HTTP Response 422: Unprocessable Entity if `eventID` or `taskList` are null or in incorrect format.

Returns HTTP Response 404: Not Found if there's no `CalendarEvent` with given `eventID`

Returns HTTP Response 400: Bad Request if there's already `TaskList` assigned to given `CalendarEvent`.

#### 3) `/api/v1.0/TaskList/tasks`
Adds sent instance of `Task` to database. 

If request succeeded: Returns `Guid`, which is ID of a newly inserted to database `Task`.

otherwise: Returns HTTP Response 422: Unprocessable Entity if sent `Task` is null or it's properties are empty.

#### 4) `/api/v1.0/TaskList/declarations`
Adds sent instance of `PersonTaskCompletionDeclaration` to database. 

If request succeeded: Returns `Guid`, which is ID of a newly inserted to database `PersonTaskCompletionDeclaration`.

otherwise:

Returns HTTP Response 422: Unprocessable Entity if sent `PersonTaskCompletionDeclaration` is null or it's properties are empty.

Returns HTTP Response 400: Bad Request if there's no such `Task` or there has been assigned maximum amount of people.

#### 5) `/api/v1.0/Calendar/events`
Adds sent instance of `CalendarEvent` to database, and adds `Notification` about it.

If request succeeded: Returns `Guid`, which is ID of a newly inserted to database `CalendarEvent`.

otherwise: Returns HTTP Response 422: Unprocessable Entity if sent `CalendarEvent` is null or it's properties are empty.

#### 6) `/api/v1.0/Calendar/categories`
Adds sent instance of `CalendarEventCategory` to database. 

If request succeeded: Returns `Guid`, which is ID of a newly inserted to database `CalendarEventCategory`.

otherwise: Returns HTTP Response 422: Unprocessable Entity if sent `CalendarEventCategory` is null or it's properties are empty.

#### 7) `/api/v1.0/Calendar/participation`
Adds sent instance of `ParticipationRegistration` to database. 

If request succeeded: Returns `Guid`, which is ID of a newly inserted to database `ParticipationRegistration`.

otherwise: Returns HTTP Response 422: Unprocessable Entity if sent `ParticipationRegistration` is null or it's properties are empty or it already exists.

#### 8) `/api/v1.0/Post/posts`
Adds sent instance of `Post` to database.

If request succeeded: Returns `Guid`, which is ID of a newly inserted to database `Post`.

otherwise: Returns HTTP Response 422: Unprocessable Entity if sent `Post` is null or it's properties are empty.

#### 9) `/api/v1.0/Post/posts/publish`
Publishes sent `Post` adds old version of it to history and adds the `Notification`

If request succeeded: Returns `Guid`, which is ID of a newly inserted to database `Post`.

otherwise: Returns HTTP Response 422: Unprocessable Entity if sent `Post` is null or it's properties are empty.


### HTTP GET

#### 1) `/api/v1.0/TaskList/tasklists?owner=...`
Returns collection of `TaskList`, where each belongs to given user, whose email/id equals to `owner`. `owner` should be string

Request always succeeds: Returns collection of `TaskList`, to which given user has access.

#### 2) `/api/v1.0/TaskList/tasklist?eventID=...`
Returns TaskList, which has been assigned to `CalendarEvent`, in which `CalendarEvent.ID` equals to `eventID`. If there's no such `TaskList`, returns null.

Important: `eventID` is `Guid`.

If request succeeded: Returns `TaskList`, which is assigned to `CalendarEvent` or `Details` equals null, when there's no such `TaskList`.

otherwise: 

Returns HTTP Response 422: Unprocessable Entity if sent `eventID` was empty.

Returns HTTP Response 404: Not Found if there's no `CalendarEvent` with given `eventID`

#### 3) `/api/v1.0/TaskList/tasklists?owner=...`
Returns TaskLists, which belongs to user with given email or belongs to `CalendarEvent` to which is registered. If there's no such `TaskList`, returns empty collection.

Important: `owner` is `string`.

Request always succeeds: Returns `TaskList`, which is assigned to `CalendarEvent` or `Details` equals null, when there's no such `TaskList`.

#### 4) `/api/v1.0/TaskList/tasks?taskListID=...`
Returns collection of `Task`, where each element belongs to `TaskList`, and `TaskList.ID` equals `taskListID`. If there's no such `Task`, returns empty collection.

Important: `taskListID` is `Guid`.

Requests always succeeds: Returns collection of `Task` assigned to given `TaskList`. If there's no, `Details` will be empty collection.

#### 5) `/api/v1.0/TaskList/declarations?taskID=...`
Returns collection of `PersonTaskCompletionDeclaration`, where `PersonTaskCompletionDeclaration.Task` equals `taskID`. IF there's none, returns empty collection.

Important: `taskID` is `Guid`.

Requests always succeeds: Returns collection of `PersonTaskCompletionDeclaration` assigned to given `Task`. If there's no, `Details` will be empty collection.

#### 6) `/api/v1.0/Calendar/events?groups=...&email=...`
Returns collection of `CalendarEvent`, which are held for given groups or given person.

Request always succeeds: Returns collection of `CalendarEvent`, where each meets condition mentioned above or empty collection if there's none.

#### 7) `/api/v1.0/Calendar/categories?group=...&email=...`
Returns collection of `CalendarEventCategory`, to which given group or person has access.

Requests always succeeds: Returns collection of `CalendarEventCategory`, to which given group or person has access or empty collection if there's none.

#### 8) `/api/v1.0/Calendar/participation?eventID=...`
Returns collection of `ParticipationRegistration`, where for each: `ParticipationRegistration.CalendarEventID` equals `eventID`.

Important: `eventID` is `Guid`

Requests always succeeds: Returns collection `ParticipationRegistration`, where each: `ParticipationRegistration.CalendarEventID` is equal to `eventID` or empty collection if there's none.

#### 9) `/api/v1.0/Post/posts?email=...&groups=...&&howMany=...&fromWhere=...`
Returns collection of `Post`, which were created for a person with given `email` or belongs to one of the `groups`. Returns given amount of `Post`, which are sorted by `PushlishTime` and is starting from `fromWhere` and are Published.

Important: `groups`should be separated by `'\n'`. `howMany` and `fromWhere` should be greater of equal to zero.

Request always succeeds: Returns described above collection of `Post`.

#### 10) `api/v1.0/Post/posts/mine?user=...`
Returns collection of `Post`, where `Post.Owner` equals `user`.

Request always succeeds: Returns described above collection of `Post`.

### HTTP DELETE

#### 1) `/api/v1.0/TaskList/tasklists`
Removes from database all mentions of a given `TaskList` and object itself.

Important: Sent values has to be in format of `Dictionary<string, string>`, like this:
```json
{
	"taskListID": "Guid of the TaskList to deletion",
	"owner": "Who tries to delete it"
}
```

If request succeeded: Returns nothing.

otherwise: 

Returns HTTP Response 404: Not Found if there's no such `TaskList`.

Returns HTTP Response 403: Forbidden if you have no right to remove it.

#### 2) `/api/v1.0/TaskList/tasks`
Removes from database all mentions of a given `Task` and object itself.

Important: Sent values has to be in format of `Dictionary<string, string>`, like this:
```json
{
	"taskId": "Guid of the Task to deletion",
	"owner": "Who tries to delete it"
}
```

If request succeeded: `Details` are empty and `Task` is removed as well as all mentions of it.

#### 3) `/api/v1.0/TaskList/declarations`
Removes from database all mentions of a given `PersonTaskCompletionDeclaration` and object itself.

Important: Sent values has to be in format of `Dictionary<string, string>`, like this:
```json
{
	"taskId": "Guid of the TaskList to deletion",
	"owner": "Who tries to delete it"
}
```

If request succeeded: Returns nothing

#### 4) `/api/v1.0/Calendar/events`
Removes from database all mentions of a given `CalendarEvent` and object itself.

Important: Sent values has to be in format of `Dictionary<string, string>`, like this:
```json
{
	"calendarEventID": "Guid of the TaskList to deletion",
	"owner": "Who tries to delete it"
}
```

If request succeeded: Returns nothing.


#### 5) `/api/v1.0/Calendar/participations`
Removes from database all mentions of a given `ParticipationRegistration` and object itself.

Important: Sent values has to be in format of `Dictionary<string, string>`, like this:
```json
{
	"calendarEventID": "Guid of the TaskList to deletion",
	"owner": "Who tries to delete it"
}
```

If request succeeded: Returns nothing.

#### 6) `/api/v1.0/Post/posts`
Removes from database `Post`

Important: Sent values has to be in format of `Post`.

If request succeeded: Returns nothing.

### HTTP PATCH

#### 1) `/api/v1.0/TaskList/tasklists`
Updates `TaskList` value in database to sent value.

Important: DO NOT change value of a `TaskList.ID`, otherwise you will not be able to update it's value!

If request succeeded: Returns nothing.

#### 2) `/api/v1.0/TaskList/tasks`
Updates `Task` value in database to sent value.

Important: DO NOT change value of a `Task.ID`, otherwise you will not be able to update it's value!

If request succeeded: Returns nothing

#### 3) `/api/v1.0/Calendar/events`
Updates `CalendarEvent` value in database to sent value.

Important: DO NOT change value of a `CalendarEvent.ID`, otherwise you will not be able to update it's value!

If request succeeded: Returns nothing.

#### 4) `/api/v1.0/Calendar/categories`
Updates `CalendarEventCategory` value in database to sent value.

Important: DO NOT change value of a `CalendarEventCategory.ID`, otherwise you will not be able to update it's value!

If request succeeded: Return nothing.

#### 5) `/api/v1.0/Post/posts/publish`
Publishes sent `Post` and if it's change, adds to history of edition.
Important: DO NOT change value of a `CalendarEventCategory.ID`, otherwise you will not be able to update it's value!

If request succeeded: Return nothing.

#### 6) `/api/v1.0/Post/posts`
Updates `Post`.

Important: DO NOT change value of a `CalendarEventCategory.ID`, otherwise you will not be able to update it's value!

If request succeeded: Return nothing.