type ParamQuery = {[key:string]: any}
export const getURLQuery = (paramQuery: ParamQuery) => {
    const query = new URLSearchParams();
    Object.keys(paramQuery).forEach((key, _) => {
        const value = paramQuery[key];
        if (value != null || value != undefined) {
            query.append(key, decodeURIComponent(value));
        }
    });
    return query.toString();
};