import { combineReducers } from "redux";

export const selectedMessageReducer = (
	selectedMessage = null,
	action: { type: string; payload: any }
) => {
	if (action.type === "MESSAGE_SELECTED") {
		return action.payload;
	}
	return selectedMessage;
};

export const messagesListReducer = (
	messages = [],
	action: { type: string; payload: any }
) => {
	if (action.type === "FETCH_MESSAGES") {
		return action.payload;
	}
	return messages;
};

export const draftmessagesListReducer = (
	draftMessages = [],
	action: { type: string; payload: any }
) => {
	if (action.type === "FETCH_DRAFTMESSAGES") {
		return action.payload;
	}
	return draftMessages;
};

export const scheduleMessagesListReducer = (
	scheduleMessages = [],
	action: { type: string; payload: any }
) => {
	if (action.type === "FETCH_SCHEDULEMESSAGES") {
		return action.payload;
	}
	return scheduleMessages;
};

export const templateMessagesListReducer = (
	templateMessages = [],
	action: { type: string; payload: any }
) => {
	if (action.type === "FETCH_TEMPLATEMESSAGES") {
		return action.payload;
	}
	return templateMessages;
};

export default combineReducers({
	messagesList: messagesListReducer,
	draftMessagesList: draftmessagesListReducer,
	selectedMessage: selectedMessageReducer,
	scheduleMessagesList: scheduleMessagesListReducer,
	templateMessagesList: templateMessagesListReducer,
});
