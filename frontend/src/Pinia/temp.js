const info = {
    name: "",
    label: "Thưởng sản lượng",
    total: 0,
    yearTotal: 0,
    quarterTotal: 0,
    visible: false,
    lines: [],
};
if (res.data?.committedLine.length) {
    // debugger
    for (let line of res.data?.committedLine) {
        const _line = {
            label: cmtType[line.committedType]
                ? `Thưởng sản lượng ${cmtType[line.committedType]} ${
                      currentQYNum[line.committedType]
                  }`
                : "Unknown",
            time_num: currentQYNum[line.committedType],
            value: 0,
        };

        const line3M = {
            label: "Thưởng 3 tháng",
            value: 0,
        };
        const line6M = {
            label: "Thưởng 6 tháng",
            value: 0,
        };
        const line9M = {
            label: "Thưởng 9 tháng",
            value: 0,
        };
        const line1Y = {
            label: "Thưởng năm",
            value: 0,
        };
        const lineOver = {
            label: "Thưởng sản lượng vượt",
            value: 0,
        };

        for (let subLine of line.committedLineSub) {
            if (subLine[getProVal(line.committedType)]) {
                _line.value += subLine[getProVal(line.committedType)];
                info.total += subLine[getProVal(line.committedType)];
                if (line.committedType == "Y") {
                    info.yearTotal +=
                        subLine[getProVal(line.committedType)];
                }
                if (line.committedType == "Q") {
                    info.quarterTotal +=
                        subLine[getProVal(line.committedType)];
                }
            }
            if (subLine.threeMonthBonus) {
                line3M.value += subLine.threeMonthBonus;
                info.total += subLine.threeMonthBonus;
                if (line.committedType == "Y") {
                    info.yearTotal += subLine.nineMonthBonus;
                }
                if (line.committedType == "Q") {
                    info.quarterTotal += subLine.nineMonthBonus;
                }
            }
            if (subLine.sixMonthBonus) {
                line6M.value += subLine.sixMonthBonus;
                info.total += subLine.sixMonthBonus;
                if (line.committedType == "Y") {
                    info.yearTotal += subLine.sixMonthBonus;
                }
                if (line.committedType == "Q") {
                    info.quarterTotal += subLine.sixMonthBonus;
                }
            }
            if (subLine.nineMonthBonus) {
                line9M.value += subLine.nineMonthBonus;
                info.total += subLine.nineMonthBonus;
                if (line.committedType == "Y") {
                    info.yearTotal += subLine.nineMonthBonus;
                }
                if (line.committedType == "Q") {
                    info.quarterTotal += subLine.nineMonthBonus;
                }
            }
            if (
                subLine.afterVolumn >= subLine.total &&
                subLine.yearBonus
            ) {
                line1Y.value += subLine.yearBonus;
                info.total += subLine.yearBonus;
                if (line.committedType == "Y") {
                    info.yearTotal += subLine.nineMonthBonus;
                }
                if (line.committedType == "Q") {
                    info.quarterTotal += subLine.nineMonthBonus;
                }
            }
            // Thưởng sản lượng vượt
            if (subLine.committedLineSubSub.length) {
                for (let subSubLine of subLine.committedLineSubSub) {
                    if (subLine.afterVolumn >= subSubLine.outPut) {
                        lineOver.value += subSubLine.bonusTotal;
                        info.total += subSubLine.bonusTotal;
                        if (line.committedType == "Y") {
                            info.yearTotal += subSubLine.bonusTotal;
                        }
                        if (line.committedType == "Q") {
                            info.quarterTotal += subSubLine.bonusTotal;
                        }
                    }
                }
            }
        }

        info.lines.push(
            ...[_line, line3M, line6M, line9M, line1Y, lineOver]
        );
    }

    tiemGiam.value = info;
    // set to purchase order store
    poStore.setData(
        {
            quy: info.quarterTotal,
            nam: info.yearTotal,
        },
        "CK"
    );
}