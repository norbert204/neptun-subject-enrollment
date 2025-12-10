import axios from 'axios';

const setAuthToken = (token) => {
  if (token) {
    axios.defaults.headers.common['Authorization'] = `Bearer ${token}`;
  } else {
    delete axios.defaults.headers.common['Authorization'];
  }
};

// initialize from localStorage if present
try {
  const tok = localStorage.getItem('accessToken');
  if (tok) setAuthToken(tok);
} catch (e) {}

export { setAuthToken };
