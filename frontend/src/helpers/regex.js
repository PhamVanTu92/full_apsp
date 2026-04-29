// src/helpers/regex.js

// Regular expression to validate email addresses
const emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$/g;

// Regular expression to validate phone numbers (VN format)
// const phoneRegex = /^(0|\+84)(3[2-9]|5[6|8|9]|7[0|6-9]|8[1-5]|9[0-9])[0-9]{7}$/g
const phoneRegex = /^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$/

// Regular expression to validate URLs
const urlRegex = /^(https?|ftp):\/\/[^\s/$.?#].[^\s]*$/i;

// Regular expression to validate postal codes (US format)
const postalCodeRegex = /^[0-9]{5}(?:-[0-9]{4})?$/;

const vehiclePlateRegex = /^[a-zA-Z0-9-]{1,256}$/;

const cccdRegex = /^[0-9]{12}$/g;

// Export the regular expressions
export {
    emailRegex,
    phoneRegex,
    urlRegex,
    postalCodeRegex,
    vehiclePlateRegex,
    cccdRegex
};