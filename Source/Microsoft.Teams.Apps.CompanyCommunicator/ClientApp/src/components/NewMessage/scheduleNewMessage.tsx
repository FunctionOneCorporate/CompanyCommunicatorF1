import * as React from "react";
import { connect } from 'react-redux';
import { RouteComponentProps } from "react-router-dom";
import { withTranslation, WithTranslation } from "react-i18next";
import {
	Input,
	TextArea,
	Radiobutton,
	RadiobuttonGroup,
} from "msteams-ui-components-react";
import { initializeIcons } from "office-ui-fabric-react/lib/Icons";
import * as AdaptiveCards from "adaptivecards";
import { Button, Loader, Dropdown, Text } from "@stardust-ui/react";
import * as microsoftTeams from "@microsoft/teams-js";

import "./newMessage.scss";
import "./teamTheme.scss";
import {
	getScheduleNotification,
	getTeams,
	updateScheduleNotification,
	duplicateScheduleNotification,
	deleteScheduleNotification,
	searchGroupsSchedule,
	getGroupsSchedule,
	verifyGroupAccessSchedule,
	imageBase64toURI
} from "../../apis/messageListApi";
import {
	getInitAdaptiveCard,
	setCardTitle,
	setCardImageLink,
	setCardSummary,
	setCardAuthor,
	//setCardBtn,
	setCardImgHeader,
	setCardImgFooter,
	//setCardBtn02,
	//deleteBtn,
	setCardActions,
} from "../AdaptiveCard/adaptiveCard";
import { getScheduleMessagesList } from  "../../actions"
import { getBaseUrl } from "../../configVariables";
import { ImageUtil } from "../../utility/imageutility";
import { TFunction } from "i18next";
import FileBase64 from '../../utility/FileBase64';

type dropdownItem = {
	key: string;
	header: string;
	content: string;
	image: string;
	team: {
		id: string;
	};
};

export interface IScheduleMessage {
	id?: string;
	title?: string;
	imageLink?: string;
	summary?: string;
	author: string;
	buttonTitle?: string;
	buttonLink?: string;
	teams: any[];
	rosters: any[];
	groups: any[];
	allUsers: boolean;
	template: boolean;
	schedule: boolean;
	scheduleDate: string;
	headerImgLink?: string;
	footerImgLink?: string;
	buttonLink2?: string;
	buttonTitle2?: string;
	nmMensagem: string;
}

export interface formState {
	title?: string;
	summary?: string;
	btnLink?: string;
	imageLink?: string;
	btnTitle?: string;
	author: string;
	card?: any;
	page: string;
	teamsOptionSelected: boolean;
	rostersOptionSelected: boolean;
	allUsersOptionSelected: boolean;
	groupsOptionSelected: boolean;
	teams?: any[];
	groups?: any[];
	exists?: boolean;
	messageId: string;
	loader: boolean;
	groupAccess: boolean;
	loading: boolean;
	noResultMessage: string;
	unstablePinned?: boolean;
	selectedTeamsNum: number;
	selectedRostersNum: number;
	selectedGroupsNum: number;
	selectedRadioBtn: string;
	selectedTeams: dropdownItem[];
	selectedRosters: dropdownItem[];
	selectedGroups: dropdownItem[];
	errorImageUrlMessage: string;
	errorButtonUrlMessage: string;
	scheduleDate: string;
	headerImgLink?: string;
	footerImgLink?: string;
	buttonLink2?: string;
	buttonTitle2?: string;
	nmMensagem: string;
}

export interface INewMessageProps extends RouteComponentProps, WithTranslation {
	getScheduleMessagesList?: any;
}

class NewMessage extends React.Component<INewMessageProps, formState> {
	readonly localize: TFunction;
	private card: any;

	constructor(props: INewMessageProps) {
		super(props);
		initializeIcons();
		this.localize = this.props.t;
		this.card = getInitAdaptiveCard(this.localize);
		this.setDefaultCard(this.card);

		this.state = {
			title: "",
			summary: "",
			author: "",
			btnLink: "",
			imageLink: "",
			btnTitle: "",
			card: this.card,
			page: "CardCreation",
			teamsOptionSelected: true,
			rostersOptionSelected: false,
			allUsersOptionSelected: false,
			groupsOptionSelected: false,
			messageId: "",
			loader: true,
			groupAccess: false,
			loading: false,
			noResultMessage: "",
			unstablePinned: true,
			selectedTeamsNum: 0,
			selectedRostersNum: 0,
			selectedGroupsNum: 0,
			selectedRadioBtn: "teams",
			selectedTeams: [],
			selectedRosters: [],
			selectedGroups: [],
			errorImageUrlMessage: "",
			errorButtonUrlMessage: "",
			scheduleDate: "",
			headerImgLink: "",
			footerImgLink: "",
			buttonLink2: "",
			buttonTitle2: "",
			nmMensagem: "",
		}
	}

	public async componentDidMount() {
		microsoftTeams.initialize();
		//- Handle the Esc key
		document.addEventListener("keydown", this.escFunction, false);
		let params = this.props.match.params;
		this.setGroupAccess();
		this.getTeamList().then(() => {
			if ("id" in params) {
				let id = params["id"];
				this.getItem(id).then(() => {
					const selectedTeams = this.makeDropdownItemList(
						this.state.selectedTeams,
						this.state.teams
					);
					const selectedRosters = this.makeDropdownItemList(
						this.state.selectedRosters,
						this.state.teams
					);
					this.setState({
						exists: true,
						messageId: id,
						selectedTeams: selectedTeams,
						selectedRosters: selectedRosters,
					});
				});
				this.getGroupData(id).then(() => {
					const selectedGroups = this.makeDropdownItems(this.state.groups);
					this.setState({
						selectedGroups: selectedGroups,
					});
				});
			} else {
				this.setState(
					{
						exists: false,
						loader: false,
					},
					() => {
						let adaptiveCard = new AdaptiveCards.AdaptiveCard();
						adaptiveCard.parse(this.state.card);
						let renderedCard = adaptiveCard.render();
						document
							.getElementsByClassName("adaptiveCardContainer")[0]
							.appendChild(renderedCard);
						if (this.state.btnLink) {
							let link = this.state.btnLink;
							adaptiveCard.onExecuteAction = function (action) {
								window.open(link, "_blank");
							};
						}
					}
				);
			}
		});
	}

	private makeDropdownItems = (items: any[] | undefined) => {
		const resultedTeams: dropdownItem[] = [];
		if (items) {
			items.forEach((element) => {
				resultedTeams.push({
					key: element.id,
					header: element.name,
					content: element.mail,
					image: ImageUtil.makeInitialImage(element.name),
					team: {
						id: element.id,
					},
				});
			});
		}
		return resultedTeams;
	};

	private makeDropdownItemList = (
		items: any[],
		fromItems: any[] | undefined
	) => {
		const dropdownItemList: dropdownItem[] = [];
		items.forEach((element) =>
			dropdownItemList.push(
				typeof element !== "string"
					? element
					: {
							key: fromItems!.find((x) => x.id === element).id,
							header: fromItems!.find((x) => x.id === element).name,
							image: ImageUtil.makeInitialImage(
								fromItems!.find((x) => x.id === element).name
							),
							team: {
								id: element,
							},
					}
			)
		);
		return dropdownItemList;
	};

	public setDefaultCard = (card: any) => {
		const titleAsString = this.localize("TitleText");
		const summaryAsString = this.localize("Summary");
		const authorAsString = this.localize("Author1");
		const buttonTitleAsString = this.localize("ButtonTitle");
		const buttonTitleAsString2 = this.localize("ButtonTitle2")

		setCardTitle(card, titleAsString);
		let imgUrl = getBaseUrl() + "/image/imagePlaceholder.png";
		let link = "https://adaptivecards.io";
		setCardImageLink(card, imgUrl);
		setCardImgHeader(card, imgUrl);
		setCardImgFooter(card, imgUrl);
		setCardSummary(card, summaryAsString);
		setCardAuthor(card, authorAsString);
		setCardActions(card, buttonTitleAsString, link, buttonTitleAsString2, link);
	};

	private getTeamList = async () => {
		try {
			const response = await getTeams();
			this.setState({
				teams: response.data,
			});
		} catch (error) {
			return error;
		}
	};

	private getGroupItems() {
		if (this.state.groups) {
			return this.makeDropdownItems(this.state.groups);
		}
		const dropdownItems: dropdownItem[] = [];
		return dropdownItems;
	}

	private setGroupAccess = async () => {
		await verifyGroupAccessSchedule()
			.then(() => {
				this.setState({
					groupAccess: true,
				});
			})
			.catch((error) => {
				const errorStatus = error.response.status;
				if (errorStatus === 403) {
					this.setState({
						groupAccess: false,
					});
				} else {
					throw error;
				}
			});
	};

	private getGroupData = async (id: number) => {
		try {
			const response = await getGroupsSchedule(id);
			this.setState({
				groups: response.data,
			});
		} catch (error) {
			return error;
		}
	};

	private getItem = async (id: number) => {
		try {
			const response = await getScheduleNotification(id);
			const scheduleMessageDetail = response.data;
			let selectedRadioButton = "teams";
			if (scheduleMessageDetail.rosters.length > 0) {
				selectedRadioButton = "rosters";
			} else if (scheduleMessageDetail.groups.length > 0) {
				selectedRadioButton = "groups";
			} else if (scheduleMessageDetail.allUsers) {
				selectedRadioButton = "allUsers";
			}
			this.setState({
				teamsOptionSelected: scheduleMessageDetail.teams.length > 0,
				selectedTeamsNum: scheduleMessageDetail.teams.length,
				rostersOptionSelected: scheduleMessageDetail.rosters.length > 0,
				selectedRostersNum: scheduleMessageDetail.rosters.length,
				groupsOptionSelected: scheduleMessageDetail.groups.length > 0,
				selectedGroupsNum: scheduleMessageDetail.groups.length,
				selectedRadioBtn: selectedRadioButton,
				selectedTeams: scheduleMessageDetail.teams,
				selectedRosters: scheduleMessageDetail.rosters,
				selectedGroups: scheduleMessageDetail.groups,
			});

			setCardImgHeader(this.card, scheduleMessageDetail.headerImgLink);
			setCardTitle(this.card, scheduleMessageDetail.title);
			setCardImageLink(this.card, scheduleMessageDetail.imageLink);
			setCardSummary(this.card, scheduleMessageDetail.summary);
			setCardAuthor(this.card, scheduleMessageDetail.author);
			setCardImgFooter(this.card, scheduleMessageDetail.footerImgLink);
			setCardActions(this.card, scheduleMessageDetail.buttonTitle, scheduleMessageDetail.buttonLink, scheduleMessageDetail.buttonTitle2, scheduleMessageDetail.buttonLink2);

			this.setState(
				{
					title: scheduleMessageDetail.title,
					summary: scheduleMessageDetail.summary,
					btnLink: scheduleMessageDetail.buttonLink,
					imageLink: scheduleMessageDetail.imageLink,
					btnTitle: scheduleMessageDetail.buttonTitle,
					author: scheduleMessageDetail.author,
					allUsersOptionSelected: scheduleMessageDetail.allUsers,
					loader: false,
					scheduleDate: scheduleMessageDetail.scheduleDate,
					nmMensagem: scheduleMessageDetail.nmMensagem,
					buttonTitle2: scheduleMessageDetail.buttonTitle2,
					buttonLink2: scheduleMessageDetail.buttonLink2,
					headerImgLink: scheduleMessageDetail.headerImgLink,
					footerImgLink: scheduleMessageDetail.footerImgLink
				},
				() => {
					this.updateCard();
				}
			);
		} catch (error) {
			return error;
		}
	};

	public componentWillUnmount() {
		document.removeEventListener("keydown", this.escFunction, false);
	}

	public render(): JSX.Element {
		if (this.state.loader) {
			return (
				<div className="Loader">
					<Loader />
				</div>
			);
		} else {
			if (this.state.page === "CardCreation") {
				return (
					<div className="taskModule">
						<div className="formContainer">
							<div className="formContentContainer">
								<Input
									className="inputField"
									value={this.state.nmMensagem}
									label={this.localize("NameMessage")}
									placeholder={this.localize("PlaceHolderName")}
									onChange={this.onNmMsgChanged}
									autoComplete="off"
								/>
								<div style={{ display: "flex", flexFlow: "row nowrap", alignItems: "flex-end" }}>
									<Input
										className="inputField"
										style={{ flexShrink: 0, flex: 10, margin: "0 0 0 3.2rem" }}
										value={this.state.headerImgLink}
										label={this.localize("ImgHeader")}
										placeholder={this.localize("ImgHeader")}
										onChange={this.onImageLinkHeaderChanged}
										errorLabel={this.state.errorImageUrlMessage}
										autoComplete="off"
									/>
									<Button
										style={{ flex: "1 1 auto", margin: "0 3.2rem 0 0" }}
										content={this.localize("ImageSend")}
										id="imageHeaderUploadBtn"
										onClick={this.clickUploadHeader.bind(this)}
										primary
									/>
									<FileBase64
										style={{ display: 'none' }}
										refProp={input => this.inputElementFileBase64Header = input}
										multiple={false}
										onDone={this.getFilesHeader.bind(this)} />
								</div>
								<Input
									className="inputField"
									value={this.state.title}
									label={this.localize("TitleText")}
									placeholder={this.localize("PlaceHolderTitle")}
									onChange={this.onTitleChanged}
									autoComplete="off"
								/>
								<TextArea
									className="inputField textArea"
									autoFocus
									placeholder={this.localize("Summary")}
									label={this.localize("Summary")}
									value={this.state.summary}
									onChange={this.onSummaryChanged}
								/>
								<div style={{display: "flex",flexFlow: "row nowrap",alignItems: "flex-end"}}>
									<Input
										className="inputField"
										style={{flexShrink: 0, flex: 10, margin: "0 0 0 3.2rem"}}
										value={this.state.imageLink}
										label={this.localize("ImageURL")}
										placeholder={this.localize("ImageURL")}
										onChange={this.onImageLinkChanged}
										errorLabel={this.state.errorImageUrlMessage}
										autoComplete="off"
									/>
									<Button
										style={{flex: "1 1 auto", margin: "0 3.2rem 0 0"}}
										content={this.localize("ImageSend")}
										id="imageUploadBtn"
										onClick={this.clickUpload.bind(this)}
										primary
									/>
									<FileBase64
										style={{ display: 'none' }}
										refProp={input => this.inputElementFileBase64 = input}
										multiple={ false }
										onDone={ this.getFiles.bind(this) } />
								</div>
								<Input
									className="inputField"
									value={this.state.author}
									label={this.localize("Author")}
									placeholder={this.localize("Author")}
									onChange={this.onAuthorChanged}
									autoComplete="off"
								/>
								<div style={{ display: "flex", flexFlow: "row nowrap", alignItems: "flex-end" }}>
									<Input
										className="inputField"
										style={{ flexShrink: 0, flex: 10, margin: "0 0 0 3.2rem" }}
										value={this.state.footerImgLink}
										label={this.localize("ImgFooter")}
										placeholder={this.localize("ImgFooter")}
										onChange={this.onImageLinkFooterChanged}
										errorLabel={this.state.errorImageUrlMessage}
										autoComplete="off"
									/>
									<Button
										style={{ flex: "1 1 auto", margin: "0 3.2rem 0 0" }}
										content={this.localize("ImageSend")}
										id="imageFooterUploadBtn"
										onClick={this.clickUploadFooter.bind(this)}
										primary
									/>
									<FileBase64
										style={{ display: 'none' }}
										refProp={input => this.inputElementFileBase64Footer = input}
										multiple={false}
										onDone={this.getFilesFooter.bind(this)} />
								</div>

								<Input
									className="inputField"
									value={this.state.btnTitle}
									label={this.localize("ButtonTitle")}
									placeholder={this.localize("ButtonTitle")}
									onChange={this.onBtnTitleChanged}
									autoComplete="off"
								/>

								<Input
									className="inputField"
									value={this.state.btnLink}
									label={this.localize("ButtonURL")}
									placeholder={this.localize("ButtonURL")}
									onChange={this.onBtnLinkChanged}
									errorLabel={this.state.errorButtonUrlMessage}
									autoComplete="off"
								/>
								<Input
									className="inputField"
									value={this.state.buttonTitle2}
									label={this.localize("ButtonTitle2")}
									placeholder={this.localize("ButtonTitle2")}
									onChange={this.onBtnTitleChanged2}
									autoComplete="off"
								/>

								<Input
									className="inputField"
									value={this.state.buttonLink2}
									label={this.localize("ButtonURL2")}
									placeholder={this.localize("ButtonURL2")}
									onChange={this.onBtnLinkChanged2}
									errorLabel={this.state.errorButtonUrlMessage}
									autoComplete="off"
								/>
							</div>
							<div className="adaptiveCardContainer"></div>
						</div>

						<div className="footerContainer">
							<div className="buttonContainer">
								<Button
									content={this.localize("Next")}
									disabled={this.isNextBtnDisabled()}
									id="saveBtn"
									onClick={this.onNext}
									primary
								/>
							</div>
						</div>
					</div>
				);
			} else if (this.state.page === "AudienceSelection") {
				return (
					<div className="taskModule">
						<div className="formContainer">
							<div className="formContentContainer">
								<h3>{this.localize("SendHeadingText")}</h3>
								<RadiobuttonGroup
									className="radioBtns"
									value={this.state.selectedRadioBtn}
									onSelected={this.onGroupSelected}>
									<Radiobutton
										name="grouped"
										value="teams"
										label={this.localize("SendToGeneralChannel")}
									/>
									<Dropdown
										hidden={!this.state.teamsOptionSelected}
										placeholder={this.localize(
											"SendToGeneralChannelPlaceHolder"
										)}
										search
										multiple
										items={this.getItems()}
										value={this.state.selectedTeams}
										onSelectedChange={this.onTeamsChange}
										noResultsMessage={this.localize("NoMatchMessage")}
									/>
									<Radiobutton
										name="grouped"
										value="rosters"
										label={this.localize("SendToRosters")}
									/>
									<Dropdown
										hidden={!this.state.rostersOptionSelected}
										placeholder={this.localize("SendToRostersPlaceHolder")}
										search
										multiple
										items={this.getItems()}
										value={this.state.selectedRosters}
										onSelectedChange={this.onRostersChange}
										unstable_pinned={this.state.unstablePinned}
										noResultsMessage={this.localize("NoMatchMessage")}
									/>
									<Radiobutton
										name="grouped"
										value="allUsers"
										label={this.localize("SendToAllUsers")}
									/>
									<div
										className={
											this.state.selectedRadioBtn === "allUsers" ? "" : "hide"
										}>
										<div className="noteText">
											<Text
												error
												content={this.localize("SendToAllUsersNote")}
											/>
										</div>
									</div>
									<Radiobutton
										name="grouped"
										value="groups"
										label={this.localize("SendToGroups")}
									/>
									<div
										className={
											this.state.groupsOptionSelected && !this.state.groupAccess
												? ""
												: "hide"
										}>
										<div className="noteText">
											<Text
												error
												content={this.localize("SendToGroupsPermissionNote")}
											/>
										</div>
									</div>
									<Dropdown
										className="hideToggle"
										hidden={
											!this.state.groupsOptionSelected ||
											!this.state.groupAccess
										}
										placeholder={this.localize("SendToGroupsPlaceHolder")}
										search={this.onGroupSearch}
										multiple
										loading={this.state.loading}
										loadingMessage={this.localize("LoadingText")}
										items={this.getGroupItems()}
										value={this.state.selectedGroups}
										onSearchQueryChange={this.onGroupSearchQueryChange}
										onSelectedChange={this.onGroupsChange}
										noResultsMessage={this.state.noResultMessage}
										unstable_pinned={this.state.unstablePinned}
									/>
									<div
										className={
											this.state.groupsOptionSelected && this.state.groupAccess
												? ""
												: "hide"
										}>
										<div className="noteText">
											<Text error content={this.localize("SendToGroupsNote")} />
										</div>
									</div>
								</RadiobuttonGroup>
								<div className="sheduleCheckBox">
									<Text content={this.localize("ScheduleDate")} />
									<Input
										type="date"
										className="InputDate"
										id="dateTxt"
										onChange={this.addDateSchedule}
										value={this.state.scheduleDate}
									/>
									{this.state.scheduleDate === "" ? <Text content={this.localize("ScheduleDateChoice")} /> : null}
								</div>
							</div>
							<div className="adaptiveCardContainer"></div>
						</div>
						<div className="footerContainer">
							<div className="buttonContainer">
								<Button
									content={this.localize("Delete")}
									id="deleteBtn"
									onClick={this.onDelete}

								/>
								<Button
									content={this.localize("Duplicate")}
									id="duplicateBtn"
									onClick={this.onDuplicate}

								/>
								<Button
									content={this.localize("Back")}
									onClick={this.onBack}
									secondary
								/>
								<Button
									content={this.localize("Save")}
									disabled={this.isSaveBtnDisabled()}
									id="saveBtn"
									onClick={this.onSave}
									primary
								/>
							</div>
						</div>
					</div>
				);
			} else {
				return <div>Error</div>;
			}
		}
	}

	private onGroupSelected = (value: any) => {
		this.setState({
			selectedRadioBtn: value,
			teamsOptionSelected: value === "teams",
			rostersOptionSelected: value === "rosters",
			groupsOptionSelected: value === "groups",
			allUsersOptionSelected: value === "allUsers",
			selectedTeams: value === "teams" ? this.state.selectedTeams : [],
			selectedTeamsNum: value === "teams" ? this.state.selectedTeamsNum : 0,
			selectedRosters: value === "rosters" ? this.state.selectedRosters : [],
			selectedRostersNum:	value === "rosters" ? this.state.selectedRostersNum : 0,
			selectedGroups: value === "groups" ? this.state.selectedGroups : [],
			selectedGroupsNum: value === "groups" ? this.state.selectedGroupsNum : 0,
		});
	};

	private isSaveBtnDisabled = () => {
		const teamsSelectionIsValid =
			(this.state.teamsOptionSelected && this.state.selectedTeamsNum !== 0) ||
			!this.state.teamsOptionSelected;
		const rostersSelectionIsValid =
			(this.state.rostersOptionSelected &&
				this.state.selectedRostersNum !== 0) ||
			!this.state.rostersOptionSelected;
		const groupsSelectionIsValid =
			(this.state.groupsOptionSelected && this.state.selectedGroupsNum !== 0) ||
			!this.state.groupsOptionSelected;
		const nothingSelected =
			!this.state.teamsOptionSelected &&
			!this.state.rostersOptionSelected &&
			!this.state.groupsOptionSelected &&
			!this.state.allUsersOptionSelected;
		return (
			!teamsSelectionIsValid ||
			!rostersSelectionIsValid ||
			!groupsSelectionIsValid ||
			nothingSelected
		);
	};

	private isNextBtnDisabled = () => {
		const nmMessage = this.state.nmMensagem;
		const btnTitle = this.state.btnTitle;
		const btnLink = this.state.btnLink;
		const btnTitle2 = this.state.buttonTitle2;
		const btnLink2 = this.state.buttonLink2;
		return !(
			nmMessage &&
			((btnTitle && btnLink) || (!btnTitle && !btnLink)) &&
			((btnTitle2 && btnLink2) || (!btnTitle2 && !btnLink2)) &&
			this.state.errorImageUrlMessage === "" &&
			this.state.errorButtonUrlMessage === ""
		);
	};

	private getItems = () => {
		const resultedTeams: dropdownItem[] = [];
		if (this.state.teams) {
			let remainingUserTeams = this.state.teams;
			if (this.state.selectedRadioBtn !== "allUsers") {
				if (this.state.selectedRadioBtn === "teams") {
					this.state.teams.filter(
						(x) =>
							this.state.selectedTeams.findIndex((y) => y.team.id === x.id) < 0
					);
				} else if (this.state.selectedRadioBtn === "rosters") {
					this.state.teams.filter(
						(x) =>
							this.state.selectedRosters.findIndex((y) => y.team.id === x.id) <
							0
					);
				}
			}
			remainingUserTeams.forEach((element) => {
				resultedTeams.push({
					key: element.id,
					header: element.name,
					content: element.mail,
					image: ImageUtil.makeInitialImage(element.name),
					team: {
						id: element.id,
					},
				});
			});
		}
		return resultedTeams;
	};

	private static MAX_SELECTED_TEAMS_NUM: number = 20;

	private onTeamsChange = (event: any, itemsData: any) => {
		if (itemsData.value.length > NewMessage.MAX_SELECTED_TEAMS_NUM) return;
		this.setState({
			selectedTeams: itemsData.value,
			selectedTeamsNum: itemsData.value.length,
			selectedRosters: [],
			selectedRostersNum: 0,
			selectedGroups: [],
			selectedGroupsNum: 0,
		});
	};

	private onRostersChange = (event: any, itemsData: any) => {
		if (itemsData.value.length > NewMessage.MAX_SELECTED_TEAMS_NUM) return;
		this.setState({
			selectedRosters: itemsData.value,
			selectedRostersNum: itemsData.value.length,
			selectedTeams: [],
			selectedTeamsNum: 0,
			selectedGroups: [],
			selectedGroupsNum: 0,
		});
	};

	private onGroupsChange = (event: any, itemsData: any) => {
		this.setState({
			selectedGroups: itemsData.value,
			selectedGroupsNum: itemsData.value.length,
			groups: [],
			selectedTeams: [],
			selectedTeamsNum: 0,
			selectedRosters: [],
			selectedRostersNum: 0,
		});
	};

	private onGroupSearch = (itemList: any, searchQuery: string) => {
		const result = itemList.filter(
			(item: { header: string; content: string }) =>
				(item.header &&
					item.header.toLowerCase().indexOf(searchQuery.toLowerCase()) !==
						-1) ||
				(item.content &&
					item.content.toLowerCase().indexOf(searchQuery.toLowerCase()) !== -1)
		);
		return result;
	};

	private onGroupSearchQueryChange = async (event: any, itemsData: any) => {
		if (!itemsData.searchQuery) {
			this.setState({
				groups: [],
				noResultMessage: "",
			});
		} else if (itemsData.searchQuery && itemsData.searchQuery.length <= 2) {
			this.setState({
				loading: false,
				noResultMessage: "No matches found.",
			});
		} else if (itemsData.searchQuery && itemsData.searchQuery.length > 2) {
			// handle event trigger on item select.
			const result =
				itemsData.items &&
				itemsData.items.find(
					(item: { header: string }) =>
						item.header.toLowerCase() === itemsData.searchQuery.toLowerCase()
				);
			if (result) {
				return;
			}

			this.setState({
				loading: true,
				noResultMessage: "",
			});

			try {
				const query = encodeURIComponent(itemsData.searchQuery);
				const response = await searchGroupsSchedule(query);
				this.setState({
					groups: response.data,
					loading: false,
					noResultMessage: "No matches found.",
				});
			} catch (error) {
				return error;
			}
		}
	};

	private onSave = () => {
		const selectedTeams: string[] = [];
		const selctedRosters: string[] = [];
		const selectedGroups: string[] = [];
		this.state.selectedTeams.forEach((x) => selectedTeams.push(x.team.id));
		this.state.selectedRosters.forEach((x) => selctedRosters.push(x.team.id));
		this.state.selectedGroups.forEach((x) => selectedGroups.push(x.team.id));

		const scheduleMessageObject: IScheduleMessage = {
			id: this.state.messageId,
			title: this.state.title,
			imageLink: this.state.imageLink,
			summary: this.state.summary,
			author: this.state.author,
			buttonTitle: this.state.btnTitle,
			buttonLink: this.state.btnLink,
			teams: selectedTeams,
			rosters: selctedRosters,
			groups: selectedGroups,
			allUsers: this.state.allUsersOptionSelected,
			schedule: true,
			template: false,
			scheduleDate: this.state.scheduleDate,
			nmMensagem: this.state.nmMensagem,
			buttonTitle2: this.state.buttonTitle2,
			buttonLink2: this.state.buttonLink2,
			headerImgLink: this.state.headerImgLink,
			footerImgLink: this.state.footerImgLink
		};

		return this.state.scheduleDate !== "" && this.editScheduleMessage(scheduleMessageObject).then(() => {
			microsoftTeams.tasks.submitTask();
			return this.props.getScheduleMessagesList();
		})
	};

	private editScheduleMessage = async (scheduleMessage: IScheduleMessage) => {
		try {
			await updateScheduleNotification(scheduleMessage);
		} catch (error) {
			return error;
		}
	};

	private onDuplicate = () => {
		const selectedTeams: string[] = [];
		const selctedRosters: string[] = [];
		const selectedGroups: string[] = [];
		this.state.selectedTeams.forEach((x) => selectedTeams.push(x.team.id));
		this.state.selectedRosters.forEach((x) => selctedRosters.push(x.team.id));
		this.state.selectedGroups.forEach((x) => selectedGroups.push(x.team.id));

		const scheduleMessageObject: IScheduleMessage = {
			id: this.state.messageId,
			title: this.state.title,
			imageLink: this.state.imageLink,
			summary: this.state.summary,
			author: this.state.author,
			buttonTitle: this.state.btnTitle,
			buttonLink: this.state.btnLink,
			teams: selectedTeams,
			rosters: selctedRosters,
			groups: selectedGroups,
			allUsers: this.state.allUsersOptionSelected,
			schedule: true,
			template: false,
			scheduleDate: this.state.scheduleDate,
			nmMensagem: this.state.nmMensagem,
			buttonTitle2: this.state.buttonTitle2,
			buttonLink2: this.state.buttonLink2,
			headerImgLink: this.state.headerImgLink,
			footerImgLink: this.state.footerImgLink
		};

		const id = scheduleMessageObject.id

		return this.state.scheduleDate !== "" && this.duplicateScheduleMessage(id).then(() => {
			microsoftTeams.tasks.submitTask();
			return this.props.getScheduleMessagesList();
		})
	};

	private duplicateScheduleMessage = async (id: number) => {
		try {
			await duplicateScheduleNotification(id);
		} catch (error) {
			return error;
		}
	};

	private onDelete = () => {
		const selectedTeams: string[] = [];
		const selctedRosters: string[] = [];
		const selectedGroups: string[] = [];
		this.state.selectedTeams.forEach((x) => selectedTeams.push(x.team.id));
		this.state.selectedRosters.forEach((x) => selctedRosters.push(x.team.id));
		this.state.selectedGroups.forEach((x) => selectedGroups.push(x.team.id));

		const scheduleMessageObject: IScheduleMessage = {
			id: this.state.messageId,
			title: this.state.title,
			imageLink: this.state.imageLink,
			summary: this.state.summary,
			author: this.state.author,
			buttonTitle: this.state.btnTitle,
			buttonLink: this.state.btnLink,
			teams: selectedTeams,
			rosters: selctedRosters,
			groups: selectedGroups,
			allUsers: this.state.allUsersOptionSelected,
			schedule: true,
			template: false,
			scheduleDate: this.state.scheduleDate,
			nmMensagem: this.state.nmMensagem,
			buttonTitle2: this.state.buttonTitle2,
			buttonLink2: this.state.buttonLink2,
			headerImgLink: this.state.headerImgLink,
			footerImgLink: this.state.footerImgLink
		};

		const id = scheduleMessageObject.id

		return this.deleteScheduleMessage(id).then(() => {
			microsoftTeams.tasks.submitTask();
			return this.props.getScheduleMessagesList();
		});
	};

	private deleteScheduleMessage = async (id: number) => {
        try {
            await deleteScheduleNotification(id);
        } catch (error) {
            return error;
        }
    }

	public escFunction(event: any) {
		if (event.keyCode === 27 || event.key === "Escape") {
			microsoftTeams.tasks.submitTask();
		}
	}

	private onNext = (event: any) => {
		this.setState(
			{
				page: "AudienceSelection",
			},
			() => {
				this.updateCard();
			}
		);
	};

	private onBack = (event: any) => {
		this.setState(
			{
				page: "CardCreation",
			},
			() => {
				this.updateCard();
			}
		);
	};

	private onNmMsgChanged = (event: any) => {
		this.setState(
			{
				nmMensagem: event.target.value,
			}
		);
	}

	private onTitleChanged = (event: any) => {
		let showDefaultCard =
			!event.target.value &&
			!this.state.imageLink &&
			!this.state.summary &&
			!this.state.author &&
			!this.state.btnTitle &&
			!this.state.btnLink &&
			!this.state.buttonTitle2 &&
			!this.state.buttonLink2 &&
			!this.state.headerImgLink &&
			!this.state.footerImgLink;

		setCardImgHeader(this.card, this.state.headerImgLink);
		setCardTitle(this.card, event.target.value);
		setCardImageLink(this.card, this.state.imageLink);
		setCardSummary(this.card, this.state.summary);
		setCardAuthor(this.card, this.state.author);
		setCardImgFooter(this.card, this.state.footerImgLink);
		setCardActions(this.card, this.state.btnTitle, this.state.btnLink, this.state.buttonTitle2, this.state.buttonLink2);
 
		this.setState(
			{
				title: event.target.value,
				card: this.card,
			},
			() => {
				if (showDefaultCard) {
					this.setDefaultCard(this.card);
				}
				this.updateCard();
			}
		);
	};

	private onImageLinkChanged = (event: any) => {
		let url = event.target.value.toLowerCase();

		function validURL(str: string) {
			var pattern = new RegExp('^(https?:\\/\\/)?'+ // protocol
			  '((([a-z\\d]([a-z\\d-]*[a-z\\d])*)\\.)+[a-z]{2,}|'+ // domain name
			  '((\\d{1,3}\\.){3}\\d{1,3}))'+ // OR ip (v4) address
			  '(\\:\\d+)?(\\/[-a-z\\d%_.~+]*)*'+ // port and path
			  '(\\?[;&a-z\\d%_.~+=-]*)?'+ // query string
			  '(\\#[-a-z\\d_]*)?$','i'); // fragment locator
			return !!pattern.test(str);
		}

		if (
			!(
				url === "" ||
				url.startsWith("https://") ||
				validURL(url) ||
				url.startsWith("data:image/png;base64,") ||
				url.startsWith("data:image/jpeg;base64,") ||
				url.startsWith("data:image/gif;base64,")
			)
		) {
			this.setState({
				errorImageUrlMessage: "URL must start with https://",
			});
		} else {
			this.setState({
				errorImageUrlMessage: "",
			});
		}

		let showDefaultCard =
			!this.state.title &&
			!event.target.value &&
			!this.state.summary &&
			!this.state.author &&
			!this.state.btnTitle &&
			!this.state.btnLink &&
			!this.state.headerImgLink &&
			!this.state.footerImgLink &&
			!this.state.buttonTitle2 &&
			!this.state.buttonLink2;

		setCardImgHeader(this.card, this.state.headerImgLink);
		setCardTitle(this.card, this.state.title);
		setCardImageLink(this.card, event.target.value);
		setCardSummary(this.card, this.state.summary);
		setCardAuthor(this.card, this.state.author);
		setCardImgFooter(this.card, this.state.footerImgLink);
		setCardActions(this.card, this.state.btnTitle, this.state.btnLink, this.state.buttonTitle2, this.state.buttonLink2);

		this.setState(
			{
				imageLink: event.target.value,
				card: this.card,
			},
			() => {
				if (showDefaultCard) {
					this.setDefaultCard(this.card);
				}
				this.updateCard();
			}
		);
	};

	private onImageLinkHeaderChanged = (event: any) => {
		let url = event.target.value.toLowerCase();

		function validURL(str: string) {
			var pattern = new RegExp('^(https?:\\/\\/)?' + // protocol
				'((([a-z\\d]([a-z\\d-]*[a-z\\d])*)\\.)+[a-z]{2,}|' + // domain name
				'((\\d{1,3}\\.){3}\\d{1,3}))' + // OR ip (v4) address
				'(\\:\\d+)?(\\/[-a-z\\d%_.~+]*)*' + // port and path
				'(\\?[;&a-z\\d%_.~+=-]*)?' + // query string
				'(\\#[-a-z\\d_]*)?$', 'i'); // fragment locator
			return !!pattern.test(str);
		}

		if (
			!(
				url === "" ||
				url.startsWith("https://") ||
				validURL(url) ||
				url.startsWith("data:image/png;base64,") ||
				url.startsWith("data:image/jpeg;base64,") ||
				url.startsWith("data:image/gif;base64,")
			)
		) {
			this.setState({
				errorImageUrlMessage: "URL must start with https://",
			});
		} else {
			this.setState({
				errorImageUrlMessage: "",
			});
		}

		let showDefaultCard =
			!this.state.title &&
			!event.target.value &&
			!this.state.summary &&
			!this.state.author &&
			!this.state.btnTitle &&
			!this.state.btnLink &&
			!this.state.imageLink &&
			!this.state.footerImgLink &&
			!this.state.buttonTitle2 &&
			!this.state.buttonLink2;

		setCardImgHeader(this.card, event.target.value);
		setCardTitle(this.card, this.state.title);
		setCardImageLink(this.card, this.state.imageLink);
		setCardSummary(this.card, this.state.summary);
		setCardAuthor(this.card, this.state.author);
		setCardImgFooter(this.card, this.state.footerImgLink);
		setCardActions(this.card, this.state.btnTitle, this.state.btnLink, this.state.buttonTitle2, this.state.buttonLink2);

		this.setState(
			{
				headerImgLink: event.target.value,
				card: this.card,
			},
			() => {
				if (showDefaultCard) {
					this.setDefaultCard(this.card);
				}
				this.updateCard();
			}
		);
	};

	private onImageLinkFooterChanged = (event: any) => {
		let url = event.target.value.toLowerCase();

		function validURL(str: string) {
			var pattern = new RegExp('^(https?:\\/\\/)?' + // protocol
				'((([a-z\\d]([a-z\\d-]*[a-z\\d])*)\\.)+[a-z]{2,}|' + // domain name
				'((\\d{1,3}\\.){3}\\d{1,3}))' + // OR ip (v4) address
				'(\\:\\d+)?(\\/[-a-z\\d%_.~+]*)*' + // port and path
				'(\\?[;&a-z\\d%_.~+=-]*)?' + // query string
				'(\\#[-a-z\\d_]*)?$', 'i'); // fragment locator
			return !!pattern.test(str);
		}

		if (
			!(
				url === "" ||
				url.startsWith("https://") ||
				validURL(url) ||
				url.startsWith("data:image/png;base64,") ||
				url.startsWith("data:image/jpeg;base64,") ||
				url.startsWith("data:image/gif;base64,")
			)
		) {
			this.setState({
				errorImageUrlMessage: "URL must start with https://",
			});
		} else {
			this.setState({
				errorImageUrlMessage: "",
			});
		}

		let showDefaultCard =
			!this.state.title &&
			!event.target.value &&
			!this.state.summary &&
			!this.state.author &&
			!this.state.btnTitle &&
			!this.state.btnLink &&
			!this.state.imageLink &&
			!this.state.headerImgLink &&
			!this.state.buttonTitle2 &&
			!this.state.buttonLink2;

		setCardImgHeader(this.card, this.state.headerImgLink);
		setCardTitle(this.card, this.state.title);
		setCardImageLink(this.card, this.state.imageLink);
		setCardSummary(this.card, this.state.summary);
		setCardAuthor(this.card, this.state.author);
		setCardImgFooter(this.card, event.target.value);
		setCardActions(this.card, this.state.btnTitle, this.state.btnLink, this.state.buttonTitle2, this.state.buttonLink2);

		this.setState(
			{
				footerImgLink: event.target.value,
				card: this.card,
			},
			() => {
				if (showDefaultCard) {
					this.setDefaultCard(this.card);
				}
				this.updateCard();
			}
		);
	};

	private onSummaryChanged = (event: any) => {
		let showDefaultCard =
			!this.state.title &&
			!this.state.imageLink &&
			!event.target.value &&
			!this.state.author &&
			!this.state.btnTitle &&
			!this.state.btnLink &&
			!this.state.headerImgLink &&
			!this.state.footerImgLink &&
			!this.state.buttonTitle2 &&
			!this.state.buttonLink2;

		setCardImgHeader(this.card, this.state.headerImgLink);
		setCardTitle(this.card, this.state.title);
		setCardImageLink(this.card, this.state.imageLink);
		setCardSummary(this.card, event.target.value);
		setCardAuthor(this.card, this.state.author);
		setCardImgFooter(this.card, this.state.footerImgLink);
		setCardActions(this.card, this.state.btnTitle, this.state.btnLink, this.state.buttonTitle2, this.state.buttonLink2);

		this.setState(
			{
				summary: event.target.value,
				card: this.card,
			},
			() => {
				if (showDefaultCard) {
					this.setDefaultCard(this.card);
				}
				this.updateCard();
			}
		);
	};

	private onAuthorChanged = (event: any) => {
		let showDefaultCard =
			!this.state.title &&
			!this.state.imageLink &&
			!this.state.summary &&
			!event.target.value &&
			!this.state.btnTitle &&
			!this.state.btnLink &&
			!this.state.headerImgLink &&
			!this.state.footerImgLink &&
			!this.state.buttonTitle2 &&
			!this.state.buttonLink2;
		setCardImgHeader(this.card, this.state.headerImgLink);
		setCardTitle(this.card, this.state.title);
		setCardImageLink(this.card, this.state.imageLink);
		setCardSummary(this.card, this.state.summary);
		setCardAuthor(this.card, event.target.value);
		setCardImgFooter(this.card, this.state.footerImgLink);
		setCardActions(this.card, this.state.btnTitle, this.state.btnLink, this.state.buttonTitle2, this.state.buttonLink2);

		this.setState(
			{
				author: event.target.value,
				card: this.card,
			},
			() => {
				if (showDefaultCard) {
					this.setDefaultCard(this.card);
				}
				this.updateCard();
			}
		);
	};

	private onBtnTitleChanged = (event: any) => {
		const showDefaultCard =
			!this.state.title &&
			!this.state.imageLink &&
			!this.state.summary &&
			!this.state.author &&
			!event.target.value &&
			!this.state.btnLink &&
			!this.state.headerImgLink &&
			!this.state.footerImgLink &&
			!this.state.buttonTitle2 &&
			!this.state.buttonLink2;
		setCardImgHeader(this.card, this.state.headerImgLink);
		setCardTitle(this.card, this.state.title);
		setCardImageLink(this.card, this.state.imageLink);
		setCardSummary(this.card, this.state.summary);
		setCardAuthor(this.card, this.state.author);
		setCardImgFooter(this.card, this.state.footerImgLink);

		if (event.target.value && this.state.btnLink) {
			setCardActions(this.card, event.target.value, this.state.btnLink, this.state.buttonTitle2, this.state.buttonLink2);
			this.setState(
				{
					btnTitle: event.target.value,
					card: this.card,
				},
				() => {
					if (showDefaultCard) {
						this.setDefaultCard(this.card);
					}
					this.updateCard();
				}
			);
		} else {
			this.setState(
				{
					btnTitle: event.target.value,
				},
				() => {
					if (showDefaultCard) {
						this.setDefaultCard(this.card);
					}
					this.updateCard();
				}
			);
		}
	};

	private onBtnTitleChanged2 = (event: any) => {
		const showDefaultCard =
			!this.state.title &&
			!this.state.imageLink &&
			!this.state.summary &&
			!this.state.author &&
			!this.state.btnTitle &&
			!this.state.btnLink &&
			!this.state.headerImgLink &&
			!this.state.footerImgLink &&
			!event.target.value &&
			!this.state.buttonLink2;
		setCardImgHeader(this.card, this.state.headerImgLink);
		setCardTitle(this.card, this.state.title);
		setCardImageLink(this.card, this.state.imageLink);
		setCardSummary(this.card, this.state.summary);
		setCardAuthor(this.card, this.state.author);
		setCardImgFooter(this.card, this.state.footerImgLink);

		if (event.target.value && this.state.btnLink) {
			setCardActions(this.card, this.state.btnTitle, this.state.btnLink, event.target.value, this.state.buttonLink2);
			this.setState(
				{
					buttonTitle2: event.target.value,
					card: this.card,
				},
				() => {
					if (showDefaultCard) {
						this.setDefaultCard(this.card);
					}
					this.updateCard();
				}
			);
		} else {
			this.setState(
				{
					buttonTitle2: event.target.value,
				},
				() => {
					if (showDefaultCard) {
						this.setDefaultCard(this.card);
					}
					this.updateCard();
				}
			);
		}
	};

	private onBtnLinkChanged = (event: any) => {
		function validURL(str: string) {
			var pattern = new RegExp('^(https?:\\/\\/)?'+ // protocol
			  '((([a-z\\d]([a-z\\d-]*[a-z\\d])*)\\.)+[a-z]{2,}|'+ // domain name
			  '((\\d{1,3}\\.){3}\\d{1,3}))'+ // OR ip (v4) address
			  '(\\:\\d+)?(\\/[-a-z\\d%_.~+]*)*'+ // port and path
			  '(\\?[;&a-z\\d%_.~+=-]*)?'+ // query string
			  '(\\#[-a-z\\d_]*)?$','i'); // fragment locator
			return !!pattern.test(str);
		}

		if (
			!(
				event.target.value === "" ||
				event.target.value.toLowerCase().startsWith("https://") ||
				validURL(event.target.value)
			)
		) {
			this.setState({
				errorButtonUrlMessage: "URL must start with https://",
			});
		} else {
			this.setState({
				errorButtonUrlMessage: "",
			});
		}

		const showDefaultCard =
			!this.state.title &&
			!this.state.imageLink &&
			!this.state.summary &&
			!this.state.author &&
			!this.state.btnTitle &&
			!event.target.value &&
			!this.state.headerImgLink &&
			!this.state.footerImgLink &&
			!this.state.buttonTitle2 &&
			!this.state.buttonLink2;
		setCardImgHeader(this.card, this.state.headerImgLink);
		setCardTitle(this.card, this.state.title);
		setCardSummary(this.card, this.state.summary);
		setCardAuthor(this.card, this.state.author);
		setCardImageLink(this.card, this.state.imageLink);
		setCardImgFooter(this.card, this.state.footerImgLink);

		if (this.state.btnTitle && event.target.value) {
			setCardActions(this.card, this.state.btnTitle, event.target.value, this.state.buttonTitle2, this.state.buttonLink2);
			this.setState(
				{
					btnLink: event.target.value,
					card: this.card,
				},
				() => {
					if (showDefaultCard) {
						this.setDefaultCard(this.card);
					}
					this.updateCard();
				}
			);
		} else {
			this.setState(
				{
					btnLink: event.target.value,
				},
				() => {
					if (showDefaultCard) {
						this.setDefaultCard(this.card);
					}
					this.updateCard();
				}
			);
		}
	};

	private onBtnLinkChanged2 = (event: any) => {
		function validURL(str: string) {
			var pattern = new RegExp('^(https?:\\/\\/)?' + // protocol
				'((([a-z\\d]([a-z\\d-]*[a-z\\d])*)\\.)+[a-z]{2,}|' + // domain name
				'((\\d{1,3}\\.){3}\\d{1,3}))' + // OR ip (v4) address
				'(\\:\\d+)?(\\/[-a-z\\d%_.~+]*)*' + // port and path
				'(\\?[;&a-z\\d%_.~+=-]*)?' + // query string
				'(\\#[-a-z\\d_]*)?$', 'i'); // fragment locator
			return !!pattern.test(str);
		}

		if (
			!(
				event.target.value === "" ||
				event.target.value.toLowerCase().startsWith("https://") ||
				validURL(event.target.value)
			)
		) {
			this.setState({
				errorButtonUrlMessage: "URL must start with https://",
			});
		} else {
			this.setState({
				errorButtonUrlMessage: "",
			});
		}

		const showDefaultCard =
			!this.state.title &&
			!this.state.imageLink &&
			!this.state.summary &&
			!this.state.author &&
			!this.state.btnTitle &&
			!this.state.btnLink &&
			!this.state.headerImgLink &&
			!this.state.footerImgLink &&
			!this.state.buttonTitle2 &&
			!event.target.value;
		setCardImgHeader(this.card, this.state.headerImgLink);
		setCardTitle(this.card, this.state.title);
		setCardSummary(this.card, this.state.summary);
		setCardAuthor(this.card, this.state.author);
		setCardImageLink(this.card, this.state.imageLink);
		setCardImgFooter(this.card, this.state.footerImgLink);

		if (this.state.btnTitle && event.target.value) {
			setCardActions(this.card, this.state.btnTitle, this.state.btnLink, this.state.buttonTitle2, event.target.value);
			this.setState(
				{
					buttonLink2: event.target.value,
					card: this.card,
				},
				() => {
					if (showDefaultCard) {
						this.setDefaultCard(this.card);
					}
					this.updateCard();
				}
			);
		} else {
			this.setState(
				{
					buttonLink2: event.target.value,
				},
				() => {
					if (showDefaultCard) {
						this.setDefaultCard(this.card);
					}
					this.updateCard();
				}
			);
		}
	};

	private updateCard = () => {
		const adaptiveCard = new AdaptiveCards.AdaptiveCard();
		adaptiveCard.parse(this.state.card);
		const renderedCard = adaptiveCard.render();
		const container = document.getElementsByClassName(
			"adaptiveCardContainer"
		)[0].firstChild;
		if (container != null) {
			container.replaceWith(renderedCard);
		} else {
			document
				.getElementsByClassName("adaptiveCardContainer")[0]
				.appendChild(renderedCard);
		}
		const link = this.state.btnLink;
		adaptiveCard.onExecuteAction = function (action) {
			window.open(link, "_blank");
		};
	};

	private addDateSchedule = (event: any) => {
		this.setState({ scheduleDate: event.target.value });
	};

	private clickUpload = () => {
		this.inputElementFileBase64.click();
	}

	private clickUploadHeader = () => {
		this.inputElementFileBase64Header.click();
	}

	private clickUploadFooter = () => {
		this.inputElementFileBase64Footer.click();
	}

	private getFiles = async (file: any) => {
		const response = await imageBase64toURI(file.base64);
		setCardImageLink(this.card, response.data);
		return this.setState({imageLink: response.data},() => {this.updateCard()}
		);
	}

	private getFilesHeader = async (file: any) => {
		const response = await imageBase64toURI(file.base64);
		setCardImgHeader(this.card, response.data);
		return this.setState({ headerImgLink: response.data }, () => { this.updateCard() });
	}

	private getFilesFooter = async (file: any) => {
		const response = await imageBase64toURI(file.base64);
		setCardImgFooter(this.card, response.data);
		return this.setState({ footerImgLink: response.data }, () => { this.updateCard() });
	} 
}

const mapStateToProps = (state: any) => {
	return { messages: state.scheduleMessagesList, selectedMessage: state.selectedMessage };
}

const newMessageWithTranslation = withTranslation()(NewMessage);
export default connect(mapStateToProps, {getScheduleMessagesList})(newMessageWithTranslation);

