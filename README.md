# FNDTAPI
Calendar and Task/TODO List REST API created for FNDT App using ASP.NET Core Web API.

## Used External Libraries
* [MongoDB Driver](https://docs.mongodb.com/drivers/csharp)

## Possible responses
All responses are object with two fields:
```json
{
	"Type": "...",
	"Details": "..."
}
```
`Type` may have one of three values: `Success`, `Warming`, `Error`.

In Details, you may have description of an Error or Warming or if `Type` is Success, value of response.

## Possible requests

### HTTP POST
#### 1) `/api/v1.0/TaskList/tasklists`
Adds sended instance of `TaskList` to database.

If request successeded: `Details` contains `Guid`, which is ID of a newly inserted to database `TaskList`

otherwise: `Details` contains description of an Error or Warming.

#### 2) `/api/v1.0/TaskList/tasklists/event`
Adds given `TaskList` to specific `CalendarEvent`. 

Important: Sended values has to be in format of `Dictionary<string, string>`, like this:
```json
{
	"eventID": "Guid of the CalendarEvent",
	"taskList": "Here you place serialized TaskList"
}
```

If request successeded: `Details` contains `Guid`, which is ID of a newly inserted to database `TaskList`, which is assigned to given `CalendarEvent`.

otherwise: `Details` contains description of an Error or Warming.

#### 3) `/api/v1.0/TaskList/tasks`
Adds sended instance of `Task` to database. 

If request successeded: `Details` contains `Guid`, which is ID of a newly inserted to database `Task`.

otherwise: `Details` contains description of an Error or Warming.


#### 4) `/api/v1.0/TaskList/declarations`
Adds sended instance of `PersonTaskCompletionDeclaration` to database. 

If request successeded: `Details` contains `Guid`, which is ID of a newly inserted to database `PersonTaskCompletionDeclaration`.

otherwise: `Details` contains description of an Error or Warming.


#### 5) `/api/v1.0/Calendar/events`
Adds sended instance of `CalendarEvent` to database.

If request successeded: `Details` contains `Guid`, which is ID of a newly inserted to database `CalendarEvent`.

otherwise: `Details` contains description of an Error or Warming.


#### 6) `/api/v1.0/Calendar/categories`
Adds sended instance of `CalendarEventCategory` to database. 

If request successeded: `Details` contains `Guid`, which is ID of a newly inserted to database `CalendarEventCategory`.

otherwise: `Details` contains description of an Error or Warming.


#### 7) `/api/v1.0/Calendar/participation`
Adds sended instance of `ParticipationRegistration` to database. 

If request successeded: `Details` contains `Guid`, which is ID of a newly inserted to database `ParticipationRegistration`.

otherwise: `Details` contains description of an Error or Warming.


### HTTP GET

#### 1) `/api/v1.0/TaskList/tasklists?owner=...`
Returns collection of `TaskList`, where each belongs to given user, whose email/id equals to `owner`. `owner` should be string

Request always succeeds: `Details` contains collection of `TaskList`, to which given user has access.

#### 2) `/api/v1.0/TaskList/tasklists?eventID=...`
Returns TaskList, which has been assigned to `CalendarEvent`, in which `CalendarEvent.ID` equals to `eventID`. If there's no such `TaskList`, returns null.

Important: `eventID` is `Guid`.

If request successeded: `Details` contains `TaskList`, which is assigned to `CalendarEvent` or `Details` equals null, when there's no such `TaskList`.

otherwise: `Details` contains description of an Error or Warming.

#### 3) `/api/v1.0/TaskList/tasks?taskListID=...`
Returns collection of `Task`, where each element belongs to `TaskList`, and `TaskList.ID` equals `taskListID`. If there's no such `Task`, returns empty collection.

Important: `taskListID` is `Guid`.

Requests always succeeds: `Details` contains collection of `Task` assigned to given `TaskList`. If there's no, `Details` will be empty collection.

#### 4) `/api/v1.0/TaskList/declarations?taskID=...`
Returns collection of `PersonTaskCompletionDeclaration`, where `PersonTaskCompletionDeclaration.Task` equals `taskID`. IF there's none, returns empty collection.

Important: `taskID` is `Guid`.

Requests always succeeds: `Details` contains collection of `PersonTaskCompletionDeclaration` assigned to given `Task`. If there's no, `Details` will be empty collection.

#### 5) `/api/v1.0/Calendar/events?month=...&year=...&groups=...&email=...`
Returns collection of `CalendarEvent`, where each `CalendarEvent.BeginDate` is in given year and month and `CalendarEvent` is held for given groups or given person.

Important: month is `int` from range <1;12> and `year` > 0

If request successeded: 'Details' contains collection of `CalendarEvent`, where each meets condition mentioned above or empty collection if there's none.

otherwise: `Details` contains description of an Error or Warming.

#### 6) `/api/v1.0/Calendar/categories?group=...&email=...`
Returns collection of `CalendarEventCategory`, to which given group or person has access.

Requests always succeeds: `Details` contains collection of `CalendarEventCategory`, to which given group or person has access or empty collection if there's none.

#### 7) `/api/v1.0/Calendar/participation?eventID=...`
Returns collection of `ParticipationRegistration`, where for each: `ParticipationRegistration.CalendarEventID` equals `eventID`.

Important: `eventID` is `Guid`

Requests always succeeds: `Details` contains collection `ParticipationRegistration`, where each: `ParticipationRegistration.CalendarEventID` is equal to `eventID` or empty collection if there's none.

### HTTP DELETE

#### 1) `/api/v1.0/TaskList/tasklists`
Removes form database all mentions of a given `TaskList` and object itself.

Important: Sended values has to be in format of `Dictionary<string, string>`, like this:
```json
{
	"taskListID": "Guid of the TaskList to deletion",
	"owner": "Who tries to delete it"
}
```

If request successeded: `Details` are empty and `TaskList` is removed as well as all mentions of it.

otherwise: `Details` contains description of an Error or Warming.

#### 2) `/api/v1.0/TaskList/tasks`
Removes form database all mentions of a given `Task` and object itself.

Important: Sended values has to be in format of `Dictionary<string, string>`, like this:
```json
{
	"taskID": "Guid of the Task to deletion",
	"owner": "Who tries to delete it"
}
```

If request successeded: `Details` are empty and `Task` is removed as well as all mentions of it.

otherwise: `Details` contains description of an Error or Warming.

#### 3) `/api/v1.0/TaskList/declarations`
Removes form database all mentions of a given `PersonTaskCompletionDeclaration` and object itself.

Important: Sended values has to be in format of `Dictionary<string, string>`, like this:
```json
{
	"taskID": "Guid of the TaskList to deletion",
	"owner": "Who tries to delete it"
}
```

If request successeded: `Details` are empty and `PersonTaskCompletionDeclaration` is removed as well as all mentions of it.

otherwise: `Details` contains description of an Error or Warming.

#### 4) `/api/v1.0/Calendar/events`
Removes form database all mentions of a given `CalendarEvent` and object itself.

Important: Sended values has to be in format of `Dictionary<string, string>`, like this:
```json
{
	"calendarEventID": "Guid of the TaskList to deletion",
	"owner": "Who tries to delete it"
}
```

If request successeded: `Details` are empty and `CalendarEvent` is removed as well as all mentions of it.

otherwise: `Details` contains description of an Error or Warming.

#### 5) `/api/v1.0/Calendar/participation`
Removes form database all mentions of a given `ParticipationRegistration` and object itself.

Important: Sended values has to be in format of `Dictionary<string, string>`, like this:
```json
{
	"calendarEventID": "Guid of the TaskList to deletion",
	"owner": "Who tries to delete it"
}
```

If request successeded: `Details` are empty and `ParticipationRegistration` is removed as well as all mentions of it.

otherwise: `Details` contains description of an Error or Warming.

### HTTP PATCH

#### 1) `/api/v1.0/TaskList/tasklists`
Updates `TaskList` value in database to sended value.

Important: DO NOT change value of a `TaskList.ID`, otherwise you will not be able to update it's value!

If request successeded: 'Details' are empty and `TaskList` value is updated.

otherwise: `Details` contains description of an Error or Warming.

#### 2) `/api/v1.0/TaskList/tasks`
Updates `Task` value in database to sended value.

Important: DO NOT change value of a `Task.ID`, otherwise you will not be able to update it's value!

If request successeded: `Details` are empty and `Task` value is updated.

otherwise: `Details` contains description of an Error or Warming.

#### 3) `/api/v1.0/Calendar/events`
Updates `CalendarEvent` value in database to sended value.

Important: DO NOT change value of a `CalendarEvent.ID`, otherwise you will not be able to update it's value!

If request successeded: `Details` are empty and `CalendarEvent` value is updated.

otherwise: `Details` contains description of an Error or Warming.

#### 4) `/api/v1.0/Calendar/categories`
Updates `CalendarEventCategory` value in database to sended value.

Important: DO NOT change value of a `CalendarEventCategory.ID`, otherwise you will not be able to update it's value!

If request successeded: `Details` are empty and `CalendarEventCategory` value is updated.

otherwise: `Details` contains description of an Error or Warming.
