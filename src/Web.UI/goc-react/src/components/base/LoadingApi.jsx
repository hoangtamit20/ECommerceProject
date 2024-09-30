import React from 'react';
import './css/LoadingApi.css';

const LoadingApi = () => {
    return (
        <div className="loading-api">
            <div className="chong-phap">
                <div className="circle circle-outer"></div>
                <div className="circle circle-middle"></div>
                <div className="circle circle-inner"></div>
                <div className="energy-ball">
                    <div className="light-glow"></div>
                    <div className="light-glow small"></div>
                    <div className="light-glow smallest"></div>
                </div>
            </div>
            <div className="star-container">
                <div className="star"></div>
                <div className="star"></div>
                <div className="star"></div>
                <div className="star"></div>
                <div className="star"></div>
                <div className="star"></div>
                <div className="star"></div>
                <div className="star"></div>
                <div className="star"></div>
            </div>
            <h5 className="loading-text">Đang xử lý ...</h5>
        </div>
    );
};

export default LoadingApi;