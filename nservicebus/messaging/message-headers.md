---
title: Message Headers
summary: Access and manipulate the built in NServiceBus headers or add custom headers.
tags: 
- Header
redirects:
- nservicebus/how-do-i-get-technical-information-about-a-message
- nservicebus/message-headers
related:
- samples/header-manipulation
---

Extra information about a message is communicated over the transport as secondary information to the message body. The mechanism for header communication is either native headers, if the transport supports that feature, or via a serialized collection of key value pairs. Message headers are very similar, in both implementation and usage, to http headers. NServiceBus uses them internally to manage messaging patters, eg pub/sub and request/response, and users of NServiceBus can use headers to implement a variety of features.


## Timestamp format

For all timestamp message headers the format is `yyyy-MM-dd HH:mm:ss:ffffff Z` and are in UTC. There is a helper class `DateTimeExtensions` in the NServiceBus core that supports converting to (`ToWireFormattedString()`) and from (`ToUtcDateTime()`) this format. This class effectively does the following. 

```
const string Format = "yyyy-MM-dd HH:mm:ss:ffffff Z";

public static string ToWireFormattedString(DateTime dateTime)
{
    return dateTime.ToUniversalTime()
        .ToString(Format, CultureInfo.InvariantCulture);
}

public static DateTime ToUtcDateTime(string wireFormattedString)
{
    return DateTime.ParseExact(wireFormattedString, Format, CultureInfo.InvariantCulture)
       .ToUniversalTime();
}
```


## Serialization Headers
This set of headers contains information to control how messages are [de-serialized](/nservicebus/serialization/) by the receiving endpoint.

 * `NServiceBus.ContentType`: The type of serialization used for the message. For example ` text/xml` or `text/json`. The `NServiceBus.ContentType` header was added in version 4.0. In some cases it may be useful to use the `NServiceBus.Version` header to determine when to use the `NServiceBus.ContentType` header. 
 * `NServiceBus.EnclosedMessageTypes`: The fully qualified type name of the enclosed message(s).


## Messaging interaction headers

Several headers are used to enable messaging interaction patters

 * `NServiceBus.MessageId`: A unique id for the current message.
 * `NServiceBus.CorrelationId`: A string used to [correlate](./message-correlation.md) reply messages to their corresponding request message. 
 * `NServiceBus.ConversationId`: The conversation that this message is part of
 * `NServiceBus.RelatedTo`: The `MessageId` that caused this message to be sent
 * `NServiceBus.MessageIntent`: Can be one of the following:
	* `Send`: Regular point-to-point send. Note that messages sent to Error queue will also have a `Send` intent.
	* `Publish`: The message is an event that has been published and will be sent to all subscribers.
	* `Subscribe`: A control message indicating that the source endpoint would like to subscribe to a specific message. 
	* `Unsubscribe`: A control message indicating that the source endpoint would like to unsubscribe to a specific message.
	* `Reply`: The message has been initiated by doing a Reply or a Return from within a Handler or a Saga.
 * `NServiceBus.ReplyToAddress`: The queue address that instructs downstream handlers or sagas where to send to when doing a Reply or Return.


### Send Headers

When a message is sent the headers will be as follows:

<!-- import HeaderWriterSend -->

The above example headers are for a Send and hence the MessageIntent header is `Send`. If this was a Publish the MessageIntent header would be `Publish`.


## Reply Headers

When doing a Reply to a Message

 * The `MessageIntent` is `Reply`.
 * The `RelatedTo` will be the same as the initiating `MessageID`.
 * The `ConversationId` will be the same as the initiating `ConversationId`.
 * The `CorrelationId` will be same as the initiating `CorrelationId`.


### Example Reply Headers

Given an initiating message with the following headers:

<!-- import HeaderWriterReply_Sending --> 

The headers of reply message will be as follows:

<!-- import HeaderWriterReply_Replying -->



## Return from a Handler

When doing a Return:

 * The Return has the same points as the Reply example from above with some additions.
 * The `ReturnMessage.ErrorCode` contains the value that was supplied to the `Bus.Return` method.
 

### Example Return Headers

Given an initiating message with the following headers:

<!-- import HeaderWriterReturn_Sending --> 

The headers of reply message will be as follows:

<!-- import HeaderWriterReturn_Returning -->


## Dispatching a message from a Saga 

When any message is dispatched from within a Saga the message will contain the following:

 * A `OriginatingSagaId` header which matches the id used as the index for the Saga Data stored in persistence.
 * A `OriginatingSagaType` which is the fully qualified type name of the saga that sent the message


### Example "Send from Saga" Headers

<!-- import HeaderWriterSaga_Sending -->


## Replying to a Saga

A message Reply is performed from a Saga will have the following headers:

 * The send headers are basically the same as a normal Reply with a few additions. 
 * Since this reply is from a secondary Saga then `OriginatingSagaId` and `OriginatingSagaType` will match the second saga
 * Since this is a Reply to a the initial Saga then the headers will contain `SagaId` and `SagaType` headers that match the initial Saga.
 

### Example "Replying to a Saga" Headers

<!-- import HeaderWriterSaga_Replying -->


## Timeout Headers

 * `NServiceBus.ClearTimeouts`: A marker header to indicate that the contained control message is requesting that timeouts be cleared for a given saga.
 * `NServiceBus.Timeout.Expire`: A timestamp that indicates when a timeout to be fired.
 * `NServiceBus.Timeout.RouteExpiredTimeoutTo`: The queue name where a timeout should be routed back to when it fires.
 * `NServiceBus.IsDeferredMessage`: A marker header to indicate that this message resulted from a Defer.


## Requesting a Timeout from a Saga

When requesting a Timeout from a Saga:
 
 * The `OriginatingSagaId`, `OriginatingSagaType`, `SagaId` and `SagaType` will all match the Saga that requested the Timeout.
 * The `Timeout.RouteExpiredTimeoutTo` header contains the queue name for where the callback for the timeout should be sent/
 * The `Timeout.Expire` header contains the timestamp for when the timeout should fire.


### Example Timeout Headers

<!-- import HeaderWriterSaga_Timeout -->


### Defer a Message

When doing a Defer the message will have similar header to a Send with a few editions:

 * The message will have `IsDeferredMessage` with the value of `true`.
 * Since the Defer feature uses the Timeouts feature the Timeout headers will exist.
 * The `Timeout.RouteExpiredTimeoutTo` header contains the queue name for where the callback for the timeout should be sent.
 * The `Timeout.Expire` header contains the timestamp for when the timeout should fire.


### Example Defer Headers

<!-- import HeaderWriterDefer -->


## Diagnostics and Informational Headers

Headers used to give visibility into "where", "when" and "by whom" Of a message. Used by [ServiceControl](/servicecontrol/), [ServiceInsight](/serviceinsight/) and [ServicePulse](/servicepulse/). 

 * `$.diagnostics`: The [host details](/nservicebus/hosting/override-hostid.md) of the endpoint where the message was being processed. This header contains three parts:
	 * `$.diagnostics.hostdisplayname`
	 * `$.diagnostics.hostid` 
	 * `$.diagnostics.originating.hostid` 
 * `NServiceBus.TimeSent`: The timestamp of when the message was sent. Used by the [Performance Counters](/nservicebus/operations/monitoring-endpoints.md).
 * `NServiceBus.OriginatingEndpoint`: The endpoint name where the message was sent from. 
 * `NServiceBus.OriginatingMachine`: The machine name where the message was sent from.
 * `NServiceBus.Version`: The NServiceBus version number.


## Audit Headers

Headers added when a message is [Audited](/nservicebus/operations/auditing.md)

 * `NServiceBus.ProcessingEnded`: The timestamp processing of a message ended.
 * `NServiceBus.ProcessingEndpoint`: Name of the endpoint where the message was processed.
 * `NServiceBus.ProcessingMachine`: Machine name of the endpoint where the message was processed.
 * `NServiceBus.ProcessingStarted`: The timestamp the processing of this message started.


### Example Audit Headers

Given an initiating message with the following headers:

<!-- import HeaderWriterAudit_Send -->

When that message fails to be processed it will be sent to the Error queue with the following headers:

<!-- import HeaderWriterAudit_Audit -->


## Retries handling headers

Headers used facilitate [Retries](/nservicebus/errors/automatic-retries.md).

 * `NServiceBus.FailedQ`: The queue at which the message processing failed.
 * `NServiceBus.FLRetries`: The number of first-level retries that has been performed for a message.
 * `NServiceBus.Retries`: Number of second-level retries that has been performed for a message.
 * `NServiceBus.Retries.Timestamp`: A timestamp used by Second Level Retries to determine if the maximum time for retrying has been reached.


## Error forwarding headers

When a message is sent to the Error queue it will have the following extra headers added to the existing headers. If retries occurred then those messages will be included with the exception of `NServiceBus.Retries`, which is removed.

 * `NServiceBus.ExceptionInfo.ExceptionType`: The [Type.FullName](https://msdn.microsoft.com/en-us/library/system.type.fullname.aspx) of the Exception. Obtained by calling `Exception.GetType().FullName`.
 * `NServiceBus.ExceptionInfo.InnerExceptionType`: The full Type name of the [InnerException](https://msdn.microsoft.com/en-us/library/system.exception.innerexception.aspx) if it exists. Obtained by calling `Exception.InnerException.GetType().FullName`.
 * `NServiceBus.ExceptionInfo.HelpLink`: The [Exception HelpLink](https://msdn.microsoft.com/en-us/library/system.exception.helplink.aspx).
 * `NServiceBus.ExceptionInfo.Message`: The [Exception Message](https://msdn.microsoft.com/en-us/library/system.exception.message.aspx).
 * `NServiceBus.ExceptionInfo.Source` The [Exception Source](https://msdn.microsoft.com/en-us/library/system.exception.source.aspx).
 * `NServiceBus.ExceptionInfo.StackTrace` The [Exception StackTrace](https://msdn.microsoft.com/en-us/library/system.exception.stacktrace.aspx).
 

### Example Error Headers

Given an initiating message with the following headers:

<!-- import HeaderWriterError_Sending -->

When that message fails to be processed it will be sent to the Error queue with the following headers:

<!-- import HeaderWriterError_Error -->


## Reading incoming Headers

Headers can be read for an incoming message.


### From a Behavior

<!-- import header-incoming-behavior -->


### From a Mutator

<!-- import header-incoming-mutator -->


### From a Handler

<!-- import header-incoming-handler -->


## Writing outgoing Headers

Headers can be written for an outgoing message.


### From a Behavior

<!-- import header-outgoing-behavior -->


### From a Mutator

<!-- import header-outgoing-mutator -->


### From a Handler

<!-- import header-outgoing-handler -->

### For all outgoing messages

NServiceBus allows you to register headers at configuration time that's then added to all outgoing messages for the endpoint.

<!-- import header-static-endpoint --> 
