export interface Brand {
    id: number;
    code: string;
    name: string;
    sapCode: string;
}

export interface Industry {
    id: number;
    code: string;
    name: string;
    brands: Brand[] | null;
    sapCode: string;
}

export interface ItemType {
    id: number;
    code: string;
    name: string;
    sapCode: string;
}

export interface CommittedLineSubSub {
    id: number;
    fatherId: number;
    outPut: number;
    bonusTotal: number;
    total: number;
    discount: number;
    isConvert: boolean;
    status: string;
}

export interface CommittedLineSub {
    id: number;
    fatherId: number;
    industryId: number;
    brandIds: number[];
    industryName: string;
    brandId: number | null;
    itemTypeIds: number[];
    itemTypes: ItemType[];
    quarter1: number;
    bonusQuarter1: number;
    totalQuarter1: number;
    quarter2: number;
    bonusQuarter2: number;
    totalQuarter2: number;
    quarter3: number;
    bonusQuarter3: number;
    totalQuarter3: number;
    quarter4: number;
    bonusQuarter4: number;
    totalQuarter4: number;
    package: number;
    // 12 tháng
    month1: number | null;
    bonusMonth1: number;
    totalMonth1: number;
    month2: number | null;
    bonusMonth2: number;
    totalMonth2: number;
    month3: number | null;
    bonusMonth3: number;
    totalMonth3: number;
    month4: number | null;
    bonusMonth4: number;
    totalMonth4: number;
    month5: number | null;
    bonusMonth5: number;
    totalMonth5: number;
    month6: number | null;
    bonusMonth6: number;
    totalMonth6: number;
    month7: number | null;
    bonusMonth7: number;
    totalMonth7: number;
    month8: number | null;
    bonusMonth8: number;
    totalMonth8: number;
    month9: number | null;
    bonusMonth9: number;
    totalMonth9: number;
    month10: number | null;
    bonusMonth10: number;
    totalMonth10: number;
    month11: number | null;
    bonusMonth11: number;
    totalMonth11: number;
    month12: number | null;
    bonusMonth12: number;
    totalMonth12: number;
    total: number;
    discount: number;
    discountYear: number;
    isCvYear: boolean;
    isConvert: boolean;
    sixMonthDiscount: number | null;
    isCvSixMonth: boolean;
    sixMonthBonus: number | null;
    threeMonthDiscount: number | null;
    threeMonthBonus: number | null;
    isCvThreeMonth: boolean;
    discountMonth: number | null;
    isCvMonth: boolean;
    nineMonthDiscount: number;
    nineMonthBonus: number | null;
    yearBonus: number | null;
    isCvNineMonth: boolean;
    currentVolumn: number;
    total12M: number;
    afterVolumn: number;
    isAchieved: boolean;
    bonusPercentage: number;
    totalBonus: number;
    industry: Industry;
    status: string;
    brand: Brand[];
    committedLineSubSub: CommittedLineSubSub[];
}

export interface CommittedLine {
    id: number;
    fatherId: number;
    committedType: "Q"|"Y";
    status: string;
    committedLineSub: CommittedLineSub[];
}

export interface CommittedResponse {
    id: number;
    committedCode: string;
    committedName: string;
    committedDescription: string;
    cardId: number;
    cardCode: string | null;
    cardName: string | null;
    committedYear: string;
    userId: number;
    creator: string | null;
    docStatus: string;
    rejectReason: string;
    committedLine: CommittedLine[];
    userType: string | null;
}
export interface CommitedData {
    industryId: number | null,
    industryName: string | null,
    currentValue: number,
    incurredValue: number,
    quotaValue: number
}
export interface CommittedShow {
    timeLabel: string,
    timeValue: number,
    data: CommitedData[]
}
