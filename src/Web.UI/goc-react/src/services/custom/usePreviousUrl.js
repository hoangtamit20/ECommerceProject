import { useEffect, useRef } from "react";
import { useLocation } from "react-router-dom";

const usePreviousUrl = () => {
  const location = useLocation();
  const previousUrlRef = useRef(location.pathname); // Khởi tạo với pathname hiện tại

  useEffect(() => {
    // Lưu giá trị hiện tại trước khi location thay đổi
    previousUrlRef.current = location.pathname;

    // Cleanup function: không cần thiết ở đây vì giá trị sẽ được cập nhật
  }, [location.pathname]);

  return previousUrlRef.current;
};

export default usePreviousUrl;