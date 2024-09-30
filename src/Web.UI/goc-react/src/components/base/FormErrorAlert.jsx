import React, { useEffect, useState } from 'react';
import { toast } from 'react-toastify';

const FormErrorAlert = ({ errors, levelClass, message }) => {
    // State to manage alert visibility
    const [isVisible, setIsVisible] = useState(false);

    // Filter errors based on errorScope
    const formSummaryError = errors.find(error => error.errorScope === 2); // FormSummary = 2
    const pageSummaryError = errors.find(error => error.errorScope === 3);  // PageSummary = 3
    const redirectPageError = errors.find(error => error.errorScope === 4); // RedirectPage = 4
    const globalError = errors.find(error => error.errorScope === 5);       // Global = 5

    if (!levelClass) {
        levelClass = 'warning';
    }

    // Set visibility for form summary alert
    useEffect(() => {
        if (formSummaryError) {
            setIsVisible(true);

            // Set a timer to hide the alert after 10 seconds
            const timer = setTimeout(() => {
                setIsVisible(false);
            }, 10000); // 10000 milliseconds = 10 seconds

            return () => clearTimeout(timer); // Cleanup on unmount
        } else {
            setIsVisible(false); // Hide alert if no form summary error
        }
    }, [formSummaryError]);

    // Handle toast notifications for PageSummary, RedirectPage, and Global
    useEffect(() => {
        if (pageSummaryError || redirectPageError || globalError) {
            const errorToShow = pageSummaryError || redirectPageError || globalError;
            toast.error(errorToShow.error);
        }
    }, [pageSummaryError, redirectPageError, globalError, levelClass]);

    // Return nothing if no form-level error exists or if the alert is not visible
    if (!formSummaryError || !isVisible) {
        return null;
    }

    return (
        <div className={`alert alert-${levelClass}`} role="alert">
            {message ? message : formSummaryError.error}
        </div>
    );
};

export default FormErrorAlert;