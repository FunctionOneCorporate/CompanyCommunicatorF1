import React from "react";
import { connect } from "react-redux";
import { withTranslation, WithTranslation } from "react-i18next";
import { Menu } from "@stardust-ui/react";
import { getBaseUrl } from "../../configVariables";
import * as microsoftTeams from "@microsoft/teams-js";
import { duplicateTemplateNotification, deleteTemplateNotification } from "../../apis/messageListApi";
import { selectMessage, getTemplateMessagesList, getMessagesList } from "../../actions";
import { TFunction } from "i18next";

export interface OverflowProps extends WithTranslation {
	message?: any;
	styles?: object;
	title?: string;
	selectMessage?: any;
	getTemplateMessagesList?: any;
	getMessagesList?: any;
}

export interface OverflowState {
	menuOpen: boolean;
}

export interface ITaskInfo {
	title?: string;
	height?: number;
	width?: number;
	url?: string;
	card?: string;
	fallbackUrl?: string;
	completionBotId?: string;
}

class Overflow extends React.Component<OverflowProps, OverflowState> {
	readonly localize: TFunction;
	constructor(props: OverflowProps) {
		super(props);
		this.localize = this.props.t;
		this.state = {
			menuOpen: false,
		};
	}

	public componentDidMount() {
		microsoftTeams.initialize();
	}

	public render(): JSX.Element {
		const items = [
			{
				key: "more",
				icon: {
					name: "more",
					outline: true,
				},
				menuOpen: this.state.menuOpen,
				active: this.state.menuOpen,
				indicator: false,
				menu: {
					items: [
						// {
						// 	key: "status",
						// 	content: this.localize("ViewStatus"),
						// 	onClick: (event: any) => {
						// 		event.stopPropagation();
						// 		this.setState({
						// 			menuOpen: false,
						// 		});
						// 		let url =
						// 			getBaseUrl() +
						// 			"/viewstatus/" +
						// 			this.props.message.id +
						// 			"?locale={locale}";
						// 		this.onOpenTaskModule(null, url, this.localize("ViewStatus"));
						// 	},
						// },
						{
                            key: 'edit',
                            content: this.localize("Edit"),
                            onClick: (event: any) => {
                                event.stopPropagation();
                                this.setState({
                                    menuOpen: false,
                                });
                                let url = getBaseUrl() + "/newmessagetemplate/" + this.props.message.id + "?locale={locale}";
                                this.onOpenTaskModule(null, url, this.localize("Edit"));
                            }
						},
						{
                            key: 'edit',
                            content: this.localize("UseModel"),
                            onClick: (event: any) => {
                                event.stopPropagation();
                                this.setState({
                                    menuOpen: false,
                                });
                                let url = getBaseUrl() + "/newmessagetemplate/" + this.props.message.id + "?locale={locale}";
                                this.onOpenTaskModule(null, url, this.localize("UseModel"));
                            }
						},
						{
							key: "duplicate",
							content: this.localize("Duplicate"),
							onClick: (event: any) => {
								event.stopPropagation();
								this.setState({
									menuOpen: false,
								});
								this.duplicateTemplateMessage(this.props.message.id).then(
									() => {
										this.props.getTemplateMessagesList();
									}
								);
							},
						},
						{
                            key: 'divider',
                            kind: 'divider',
                        },
						{
                            key: 'delete',
                            content: this.localize("Delete"),
                            onClick: (event: any) => {
                                event.stopPropagation();
                                this.setState({
                                    menuOpen: false,
                                });
                                this.deleteTemplateMessage(this.props.message.id).then(() => {
                                    this.props.getTemplateMessagesList();
                                });
                            }
                        },
					],
				},
				onMenuOpenChange: (e: any, { menuOpen }: any) => {
					this.setState({
						menuOpen: !this.state.menuOpen,
					});
				},
			},
		];

		return (
			<Menu
				className="menuContainer"
				iconOnly
				items={items}
				styles={this.props.styles}
				title={this.props.title}
			/>
		);
	}

	private onOpenTaskModule = (event: any, url: string, title: string) => {
		let taskInfo: ITaskInfo = {
			url: url,
			title: title,
			height: 530,
			width: 1000,
			fallbackUrl: url,
		};
		let submitHandler = (err: any, result: any) => {
            this.props.getTemplateMessagesList().then(() => {
                this.props.getMessagesList();
            });
        };

        microsoftTeams.tasks.startTask(taskInfo, submitHandler);
	};

	private duplicateTemplateMessage = async (id: number) => {
		try {
			await duplicateTemplateNotification(id);
		} catch (error) {
			return error;
		}
	};

	private deleteTemplateMessage = async (id: number) => {
        try {
            await deleteTemplateNotification(id);
        } catch (error) {
            return error;
        }
    }
}

const mapStateToProps = (state: any) => {
	return { messagesList: state.messagesList };
};

const overflowWithTranslation = withTranslation()(Overflow);
export default connect(mapStateToProps, {
	selectMessage,
	getTemplateMessagesList,
	getMessagesList,
})(overflowWithTranslation);
