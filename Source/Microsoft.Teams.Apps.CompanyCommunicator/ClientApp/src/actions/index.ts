import { formatDate } from "../i18n";
import {
	getSentNotifications,
	getDraftNotifications,
	getScheduleNotifications,
	getTemplateNotifications,
} from "../apis/messageListApi";

type Notification = {
	createdDateTime: string;
	failed: number;
	id: string;
	isCompleted: boolean;
	sentDate: string;
	sendingStartedDate: string;
	sendingDuration: string;
	succeeded: number;
	throttled: number;
	title: string;
	totalMessageCount: number;
};

export const selectMessage = (message: any) => {
	return {
		type: "MESSAGE_SELECTED",
		payload: message,
	};
};

export const getMessagesList = () => async (dispatch: any) => {
	const response = await getSentNotifications();
	const notificationList: Notification[] = response.data;
	notificationList.forEach((notification) => {
		notification.sendingStartedDate = formatDate(
			notification.sendingStartedDate
		);
		notification.sentDate = formatDate(notification.sentDate);
	});
	dispatch({ type: "FETCH_MESSAGES", payload: notificationList });
};

export const getDraftMessagesList = () => async (dispatch: any) => {
	const response = await getDraftNotifications();
	dispatch({ type: "FETCH_DRAFTMESSAGES", payload: response.data });
};

export const getScheduleMessagesList = () => async (dispatch: any) => {
	const response = await getScheduleNotifications();
	dispatch({ type: "FETCH_SCHEDULEMESSAGES", payload: response.data });
};

export const getTemplateMessagesList = () => async (dispatch: any) => {
	const response = await getTemplateNotifications();
	dispatch({ type: "FETCH_TEMPLATEMESSAGES", payload: response.data });
};
