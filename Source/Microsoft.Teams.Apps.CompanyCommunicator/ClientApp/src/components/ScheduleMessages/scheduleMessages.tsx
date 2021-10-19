import * as React from "react";
import { connect } from "react-redux";
import { withTranslation, WithTranslation } from "react-i18next";
import { initializeIcons } from "office-ui-fabric-react/lib/Icons";
import { Loader } from "@stardust-ui/react";
import * as microsoftTeams from "@microsoft/teams-js";

import {
	getScheduleMessagesList,
} from "../../actions";
import { getBaseUrl } from "../../configVariables";
import { TFunction } from "i18next";

import { Calendar, momentLocalizer } from "react-big-calendar";
import moment from "moment";
import "react-big-calendar/lib/css/react-big-calendar.css";

import "./scheduleMessages.scss";

const localizer = momentLocalizer(moment);

function isStringNullOrWhiteSpace(str) {
	return str === undefined || str === null
		|| typeof str !== 'string'
		|| str.match(/^ *$/) !== null;
};

export interface ITaskInfo {
	title?: string;
	height?: number;
	width?: number;
	url?: string;
	card?: string;
	fallbackUrl?: string;
	completionBotId?: string;
}

export interface IMessage {
	id: string;
	title: string;
	scheduleDate: string;
	recipients: string;
	acknowledgements?: string;
	reactions?: string;
	responses?: string;
	nmMensagem?: string;
}

export interface IMessageProps extends WithTranslation {
	scheduleMessages: IMessage[];
	getScheduleMessagesList?: any;
	getMessagesList?: any;
}

export interface IMessageState {
	message: IMessage[];
	itemsAccount: number;
	loader: boolean;
	teamsTeamId?: string;
	teamsChannelId?: string;

}

class ScheduleMessages extends React.Component<IMessageProps, IMessageState> {
	readonly localize: TFunction;
	private interval: any;
	private isOpenTaskModuleAllowed: boolean;
	constructor(props: IMessageProps) {
		super(props);
		initializeIcons();
		this.localize = this.props.t;
		this.isOpenTaskModuleAllowed = true;
		this.state = {
			message: props.scheduleMessages,
			itemsAccount: this.props.scheduleMessages.length,
			loader: true,
			teamsTeamId: "",
			teamsChannelId: "",
		};
	}

	public componentDidMount() {
		microsoftTeams.initialize();
		microsoftTeams.getContext((context) => {
			this.setState({
				teamsTeamId: context.teamId,
				teamsChannelId: context.channelId,
			});
		});
		this.props.getScheduleMessagesList();
		this.interval = setInterval(() => {
			this.props.getScheduleMessagesList();
		}, 60000);
 		//- Handle the Esc key
		document.addEventListener("keydown", this.escFunction, false);
	}

	public componentWillReceiveProps(nextProps: any) {
		this.setState({
			message: nextProps.scheduleMessages,
			loader: false,
		});
	}

	public componentWillUnmount() {
		document.removeEventListener("keydown", this.escFunction, false);
		clearInterval(this.interval);
	}

	public escFunction(event: any) {
		if (event.keyCode === 27 || event.key === "Escape") {
			microsoftTeams.tasks.submitTask();
		}
	}

	public render() {

		const allScheduleMessages = this.state.message.map((item) => ({
				start: moment(item.scheduleDate).toDate(),
				end: moment(item.scheduleDate)
					.add(1, "days")
					.toDate(),				
				title: isStringNullOrWhiteSpace(item.title) ? item.nmMensagem : item.title,
				url: getBaseUrl() + "/newmessageschedule/" + item.id + "?locale={locale}",
				selectable: true
			}));

		if (this.state.loader) {
			return <Loader />;
		} else if (this.state.message.length === 0) {
			return (
				<div className="results">{this.localize("EmptyScheduleMessages")}</div>
			);
		} else {
			return(
				<div className="calendar__container listContainer">
					<Calendar
						className="calendar__component"
						localizer={localizer}
						defaultDate={new Date()}
						defaultView="month"
						events={allScheduleMessages}
						onSelectEvent={event => this.onOpenTaskModule(null, event.url, this.localize("EditMessage"))}
						style={{ minHeight: "50vw", height: "100%", width: "100%" }}
					/>
				</div>
			);
		}
	}
	public onOpenTaskModule = (event: any, url: string, title: string) => {
		if (this.isOpenTaskModuleAllowed) {
			this.isOpenTaskModuleAllowed = false;
			let taskInfo: ITaskInfo = {
				url: url,
				title: title,
				height: 530,
				width: 1000,
				fallbackUrl: url,
			};

			let submitHandler = (err: any, result: any) => {
				this.isOpenTaskModuleAllowed = true;
			};

			microsoftTeams.tasks.startTask(taskInfo, submitHandler);
		}
	};
}


const mapStateToProps = (state: any) => {
	return {
		scheduleMessages: state.scheduleMessagesList,
	};
};

const scheduleMessagesWithTranslation = withTranslation()(ScheduleMessages);
export default connect(mapStateToProps, {
	getScheduleMessagesList,
})(scheduleMessagesWithTranslation);
