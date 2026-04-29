const format = {
    // Chuyển đổi đối tượng Date thành chuỗi ngày định dạng "dd/mm/yyyy"
    formatDate: (_date) => {
        if (!_date) return '';
        const date = new Date(_date);
        const day = date.getDate().toString().padStart(2, '0');
        const month = (date.getMonth() + 1).toString().padStart(2, '0'); // Lưu ý rằng getMonth() trả về tháng bắt đầu từ 0
        const year = date.getFullYear();
        return `${day}/${month}/${year}`;
    },

    // Chuyển đổi chuỗi ngày định dạng "dd/mm/yy" thành đối tượng Date
    toDate: (_date) => {
        const [day, month, year] = _date.split('/');
        return new Date(year, month - 1, day);
    },

    // Chuyển đổi đối tượng Date thành chuỗi ngày và giờ định dạng "dd/mm/yyyy" và "hh:mm"
    DateTime: (_date) => {
        if (!_date)
            return {
                date: '',
                time: ''
            };

        const date = new Date(_date);
        const day = date.getDate().toString().padStart(2, '0');
        const month = (date.getMonth() + 1).toString().padStart(2, '0');
        const year = date.getFullYear();
        const hour = date.getHours().toString().padStart(2, '0');
        const minute = date.getMinutes().toString().padStart(2, '0');
        return {
            date: `${day}/${month}/${year}`,
            date_format: `${year}/${month}/${day}`,
            time: `${hour}:${minute}`,
            dateTime: `${day}/${month}/${year} ${hour}:${minute}`,
            dateTimeFile: `${day}_${month}_${year}_${hour}:${minute}`
        };
    },
    DateTimePlusUTC: (_date) => { // lấy ngày giờ UTC +0
        const date = new Date(_date);
        const day = String(date.getUTCDate()).padStart(2, '0');
        const month = String(date.getUTCMonth() + 1).padStart(2, '0');
        const year = date.getUTCFullYear();
        const hours = String(date.getUTCHours()).padStart(2, '0');
        const minutes = String(date.getUTCMinutes()).padStart(2, '0');
        const seconds = String(date.getUTCSeconds()).padStart(2, '0');
        return {
            date: `${day}/${month}/${year}`,
            time: `${hours}:${minutes}`,
            timeSecond: `${hours}:${minutes}:${seconds}`,
        }
    },

    // Định dạng số thành chuỗi tiền tệ với dấu phẩy phân cách hàng nghìn và hai chữ số thập phân
    FormatCurrency: (num, decimalPlaces = 2) => {
        if (num == undefined || num == null) return '';
        let formattedNumber = Number.parseFloat(num)
            .toFixed(decimalPlaces)
            .replaceAll(/\B(?=(\d{3})+(?!\d))/g, ',');
        return formattedNumber;
    }
};

export default format;
