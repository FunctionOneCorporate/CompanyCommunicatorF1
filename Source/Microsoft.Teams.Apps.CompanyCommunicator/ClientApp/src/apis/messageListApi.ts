import axios from "./axiosJWTDecorator";
import { getBaseUrl } from "../configVariables";

let baseAxiosUrl = getBaseUrl() + "/api";

export const getSentNotifications = async (): Promise<any> => {
	let url = baseAxiosUrl + "/sentnotifications";
	return await axios.get(url);
};

export const getDraftNotifications = async (): Promise<any> => {
	let url = baseAxiosUrl + "/draftnotifications";
	return await axios.get(url);
};

export const verifyGroupAccessDraft = async (): Promise<any> => {
	let url = baseAxiosUrl + "/groupdata/verifyaccess";
	return await axios.get(url, false);
};

export const getGroupsDraft = async (id: number): Promise<any> => {
	let url = baseAxiosUrl + "/groupdata/" + id;
	return await axios.get(url);
};

export const searchGroupsDraft = async (query: string): Promise<any> => {
	let url = baseAxiosUrl + "/groupdata/search/" + query;
	return await axios.get(url);
};

export const verifyGroupAccessSchedule = async (): Promise<any> => {
	let url = baseAxiosUrl + "/groupdataschedule/verifyaccess";
	return await axios.get(url, false);
};

export const getGroupsSchedule = async (id: number): Promise<any> => {
	let url = baseAxiosUrl + "/groupdataschedule/" + id;
	return await axios.get(url);
};

export const searchGroupsSchedule = async (query: string): Promise<any> => {
	let url = baseAxiosUrl + "/groupdataschedule/search/" + query;
	return await axios.get(url);
};

export const verifyGroupAccessTemplate = async (): Promise<any> => {
	let url = baseAxiosUrl + "/groupdatatemplate/verifyaccess";
	return await axios.get(url, false);
};

export const getGroupsTemplate = async (id: number): Promise<any> => {
	let url = baseAxiosUrl + "/groupdatatemplate/" + id;
	return await axios.get(url);
};

export const searchGroupsTemplate = async (query: string): Promise<any> => {
	let url = baseAxiosUrl + "/groupdatatemplate/search/" + query;
	return await axios.get(url);
};

export const exportNotification = async (id: string): Promise<any> => {
	let url = baseAxiosUrl + "/exportnotification/" + id;
	return await axios.put(url, null, false);
};

export const getSentNotification = async (id: number): Promise<any> => {
	let url = baseAxiosUrl + "/sentnotifications/" + id;
	return await axios.get(url);
};

export const getDraftNotification = async (id: number): Promise<any> => {
	let url = baseAxiosUrl + "/draftnotifications/" + id;
	return await axios.get(url);
};

export const deleteDraftNotification = async (id: number): Promise<any> => {
	let url = baseAxiosUrl + "/draftnotifications/" + id;
	return await axios.delete(url);
};

export const duplicateDraftNotification = async (id: number): Promise<any> => {
	let url = baseAxiosUrl + "/draftnotifications/duplicates/" + id;
	return await axios.post(url);
};

export const sendDraftNotification = async (payload: {}): Promise<any> => {
	let url = baseAxiosUrl + "/sentnotifications";
	return await axios.post(url, payload);
};

export const updateDraftNotification = async (payload: {}): Promise<any> => {
	let url = baseAxiosUrl + "/draftnotifications";
	return await axios.put(url, payload);
};

export const createDraftNotification = async (payload: {}): Promise<any> => {
	let url = baseAxiosUrl + "/draftnotifications";
	return await axios.post(url, payload);
};

export const getTeams = async (): Promise<any> => {
	let url = baseAxiosUrl + "/teamdata";
	return await axios.get(url);
};

export const getConsentSummaries = async (id: number): Promise<any> => {
	let url = baseAxiosUrl + "/draftnotifications/consentSummaries/" + id;
	return await axios.get(url);
};

export const sendPreview = async (payload: {}): Promise<any> => {
	let url = baseAxiosUrl + "/draftnotifications/previews";
	return await axios.post(url, payload);
};

export const getAuthenticationConsentMetadata = async (
	windowLocationOriginDomain: string,
	login_hint: string
): Promise<any> => {
	let url = `${baseAxiosUrl}/authenticationMetadata/consentUrl?windowLocationOriginDomain=${windowLocationOriginDomain}&loginhint=${login_hint}`;
	return await axios.get(url, undefined, false);
};
//schelule
//Busca todos os schedules
export const getScheduleNotifications = async (): Promise<any> => {
	let url = baseAxiosUrl + "/scheduleNotifications";
	return await axios.get(url);
};
//Busca o schedule por id
export const getScheduleNotification = async (id: number): Promise<any> => {
	let url = baseAxiosUrl + "/scheduleNotifications/" + id;
	return await axios.get(url);
};
//Duplica o schedule
export const duplicateScheduleNotification = async (
	id: number
): Promise<any> => {
	let url = baseAxiosUrl + "/scheduleNotifications/duplicates/" + id;
	return await axios.post(url);
};
//Delete o schedule
export const deleteScheduleNotification = async (id: number): Promise<any> => {
	let url = baseAxiosUrl + "/scheduleNotifications/" + id;
	return await axios.delete(url);
};
//Atualiza o schedule
export const updateScheduleNotification = async (payload: {}): Promise<any> => {
	let url = baseAxiosUrl + "/scheduleNotifications";
	return await axios.put(url, payload);
};
//Cria um schedule
export const createScheduleNotification = async (payload: {}): Promise<any> => {
	let url = baseAxiosUrl + "/scheduleNotifications";
	return await axios.post(url, payload);
};

//Template
//Busca todos os Templates
export const getTemplateNotifications = async (): Promise<any> => {
	let url = baseAxiosUrl + "/templateNotifications";
	return await axios.get(url);
};
//Duplica o schedule
export const duplicateTemplateNotification = async (
	id: number
): Promise<any> => {
	let url = baseAxiosUrl + "/templateNotifications/duplicates/" + id;
	return await axios.post(url);
};
//Busca o Templates por id
export const getTemplateNotification = async (id: number): Promise<any> => {
	let url = baseAxiosUrl + "/templateNotifications/" + id;
	return await axios.get(url);
};
//Delete o templates
export const deleteTemplateNotification = async (id: number): Promise<any> => {
	let url = baseAxiosUrl + "/templateNotifications/" + id;
	return await axios.delete(url);
};
//Atualiza o Template
export const updateTemplateNotification = async (payload: {}): Promise<any> => {
	let url = baseAxiosUrl + "/templateNotifications";
	return await axios.put(url, payload);
};
//Cria um Template
export const createTemplateNotification = async (payload: {}): Promise<any> => {
	let url = baseAxiosUrl + "/templateNotifications";
	return await axios.post(url, payload);
};

//Envia a imagem como base64 e retorna a url da imagem armazenada
export const imageBase64toURI = async (base64: string): Promise<any> => {
	let url = baseAxiosUrl + "/image";
	return await axios.post(url, { "img": base64 });
};