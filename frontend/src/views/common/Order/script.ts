const stt = {
    DXL: {
        label: 'Đang xử lý',
        class: 'text-yellow-700 bg-yellow-200'
    },
    DXN: {
        class: 'text-teal-700 bg-teal-200',
        label: 'Đã xác nhận'
    },
    HUY: {
        class: 'text-red-500 bg-red-100',
        label: 'Đã hủy'
    },
    HUY2: {
        class: 'text-red-500 border-red-500 bg-white border-1',
        label: 'Đã hủy'
    },
    DGH: {
        class: 'text-blue-700 bg-blue-200',
        label: 'Đang giao hàng'
    },
    DHT: {
        class: 'text-green-700 bg-green-200',
        label: 'Đơn hàng hoàn thành'
    },
    CTT: {
        class: 'text-yellow-50 bg-yellow-700',
        label: 'Chờ thanh toán'
    },
    TTN: {
        class: 'text-yellow-700 bg-white border-1 border-yellow-700',
        label: 'Chờ xử lý'
    },
    // UNC: {
    //     class: "text-green-700 bg-white border-1 border-green-700",
    //     label: "Đã thanh toán",
    // },
    CXN: {
        class: 'text-orange-700 bg-white border-1 border-orange-700',
        label: 'Chờ xác nhận'
    },
    DTT: {
        class: 'text-green-700 bg-white border-1 border-green-700',
        label: 'Đã thanh toán'
    },
    DGHR: {
        class: 'text-green-700 bg-white border-1 border-green-700',
        label: 'Đã giao hàng'
    },
    DONG: {
        class: 'text-white  bg-gray-500 border-1 border-gray-500',
        label: 'Đóng'
    }
};

export function fnum(num: number, decimalPlaces: number = 0, suffix: string = ''): string {
    if (isNaN(num) || !num) {
        num = 0;
    }
    let formattedNum = num;
    if (decimalPlaces > 0) {
        let multiplier = Math.pow(10, decimalPlaces);
        formattedNum = Math.round(num * multiplier) / multiplier;
    } else {
        formattedNum = Math.round(num);
    }
    // let formattedNum = decimalPlaces > 0
    //     ? num.toFixed(decimalPlaces)
    //     : Intl.NumberFormat().format(num);
    return `${Intl.NumberFormat('us-US', {
        minimumFractionDigits: decimalPlaces > 0 ? decimalPlaces : 0,
        maximumFractionDigits: decimalPlaces > 0 ? decimalPlaces : 1
    }).format(formattedNum)}${suffix}`;
}

export const formatStatus = (data: string) => {
    return stt[data as keyof typeof stt] || { class: 'surface-100', label: data };
};
