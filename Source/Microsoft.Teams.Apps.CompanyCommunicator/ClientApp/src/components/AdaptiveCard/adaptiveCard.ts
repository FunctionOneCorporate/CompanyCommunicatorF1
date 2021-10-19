import { TFunction } from "i18next";

function isStringNullOrWhiteSpace(str: string| undefined) {
    return str === undefined || str === null
        || typeof str !== 'string'
        || str.match(/^ *$/) !== null;
};

export const getInitAdaptiveCard = (t: TFunction) => {
    const titleTextAsString = t("TitleText");
    return (
        {
            "type": "AdaptiveCard",
            "body": [
                {
                    "type": "Image",
                    "spacing": "Default",
                    "url": "",
                    "size": "Stretch",
                    "width": "400px",
                    "height":"100px",
                    "altText": ""
                },
                {
                    "type": "TextBlock",
                    "weight": "Bolder",
                    "text": titleTextAsString,
                    "size": "ExtraLarge",
                    "wrap": true
                },
                {
                    "type": "TextBlock",
                    "text": "",
                    "wrap": true
                },
                {
                    "type": "Image",
                    "spacing": "Default",
                    "url": "",
                    "size": "Stretch",
                    "width": "400px",
                    "altText": ""
                },
                {
                    "type": "TextBlock",
                    "wrap": true,
                    "size": "Small",
                    "weight": "Lighter",
                    "text": ""
                },
                {
                    "type": "Image",
                    "spacing": "Default",
                    "url": "",
                    "size": "Stretch",
                    "width": "400px",
                    "height": "100px",
                    "altText": ""
                }
            ],
            "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
            "version": "1.2",
        }
    );
}

// Get image header.
export const getCardImgHeader = (card: any) => {
    return card.body[0].url;
}

// Set image header.
export const setCardImgHeader = (card: any, imageLink?: string) => {
    card.body[0].url = imageLink;
}

// Get title.
export const getCardTitle = (card: any) => {
    return card.body[1].text;
}

// Set title.
export const setCardTitle = (card: any, title?: string) => {
    card.body[1].text = title;
}

// Get summary.
export const getCardSummary = (card: any) => {
    return card.body[2].text;
}

// Set summary.
export const setCardSummary = (card: any, summary?: string) => {
    card.body[2].text = summary;
}

// Get image.
export const getCardImageLink = (card: any) => {
    return card.body[3].url;
}

// Set image
export const setCardImageLink = (card: any, imageLink?: string) => {
    card.body[3].url = imageLink;
}

// Get author.
export const getCardAuthor = (card: any) => {
    return card.body[4].text;
}

// Set author.
export const setCardAuthor = (card: any, author?: string) => {
    card.body[4].text = author;
}

// Get image footer.
export const getCardImgFooter = (card: any) => {
    return card.body[5].url;
}

// Set image footer.
export const setCardImgFooter = (card: any, imageLink?: string) => {
    card.body[5].url = imageLink;
}

// Get button title.
export const getCardBtnTitle = (card: any) => {
    return card.actions[0].title;
}

// Get button link URL.
export const getCardBtnLink = (card: any) => {
    return card.actions[0].url;
}

// Get button title 02.
export const getCardBtnTitle2 = (card: any) => {
    return card.actions[1].title;
}

// Get button link URL 02.
export const getCardBtnLink2 = (card: any) => {
    return card.actions[1].url;
}

// Set button 01.
//export const setCardBtn = (card: any, buttonTitle?: string, buttonLink?: string) => {
//    if (!isStringNullOrWhiteSpace(buttonTitle) && !isStringNullOrWhiteSpace(buttonLink))
//    {
//        card.actions[0].title = buttonTitle;
//        card.actions[0].url = buttonLink;
//        //card.actions = [
//        //    {
//        //        "type": "Action.OpenUrl",
//        //        "title": buttonTitle,
//        //        "url": buttonLink
//        //    }
//        //];

//        console.log("card inserido: " + buttonLink + " link: " + buttonLink);
//    } 
//}

// Set button 02.
//export const setCardBtn02 = (card: any, buttonTitle?: string, buttonLink?: string) => {
//    if (!isStringNullOrWhiteSpace(buttonTitle) && !isStringNullOrWhiteSpace(buttonLink))
//    {
//        card.actions[1].title = buttonTitle;
//        card.actions[1].url = buttonLink;
//        //card.actions = [
//        //    {
//        //        "type": "Action.OpenUrl",
//        //        "title": buttonTitle,
//        //        "url": buttonLink
//        //    }
//        //];

//        console.log("card inserido: " + buttonLink + " link: " + buttonLink);
//    }
//}


export const setCardActions = (card: any, buttonTitle01?: string, buttonLink01?: string, buttonTitle02?: string, buttonLink02?: string) =>
{
    let btn01: boolean = false;
    let btn02: boolean = false;

    delete card.actions;

    if (!isStringNullOrWhiteSpace(buttonTitle01) && !isStringNullOrWhiteSpace(buttonLink01)) {
        btn01 = true;
    }

    if (!isStringNullOrWhiteSpace(buttonTitle02) && !isStringNullOrWhiteSpace(buttonLink02)) {
        btn02 = true;
    }

    if (btn01 == true && btn02 == true) {
        card.actions = [
            {
                "type": "Action.OpenUrl",
                "title": buttonTitle01,
                "url": buttonLink01
            },
            {
                "type": "Action.OpenUrl",
                "title": buttonTitle02,
                "url": buttonLink02
            }
        ];
    } else {
        if (btn01 == true && btn02 == false) {
            card.actions = [
                {
                    "type": "Action.OpenUrl",
                    "title": buttonTitle01,
                    "url": buttonLink01
                }
            ];
        } else if (btn01 == false && btn02 == true) {
            card.actions = [
                {
                    "type": "Action.OpenUrl",
                    "title": buttonTitle02,
                    "url": buttonLink02
                }
            ];
        }
    }
    
}