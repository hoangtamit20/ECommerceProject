import { useEffect } from 'react';
import { useLocation } from 'react-router-dom';
import { LocalStorageService } from './LocalStorageService'; // adjust this path accordingly

const RouteListener = () => {
    const location = useLocation();

    useEffect(() => {
        // Update the previousUrl each time the location (route) changes
        LocalStorageService.setPreviousUrl(location.pathname);
        console.log(LocalStorageService.getPreviousUrl());
    }, [location]);

    return null;
};

export default RouteListener;