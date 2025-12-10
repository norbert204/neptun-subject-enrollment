import axios from 'axios';
import { setAuthToken } from './axiosConfig';

const LOGIN_URL = '/api/Auth/login';
const REFRESH_URL = '/api/Auth/refresh-token';

export const login = async (neptunCode, password) => {
  const payload = { NeptunCode: neptunCode, Password: password };
  const resp = await axios.post(LOGIN_URL, payload);
  const data = resp.data;
  // data: { accessToken, refreshToken }
  try { localStorage.setItem('accessToken', data.accessToken); } catch (e) {}
  try { localStorage.setItem('refreshToken', data.refreshToken); } catch (e) {}
  setAuthToken(data.accessToken);
  return data;
};

export const logout = () => {
  try { localStorage.removeItem('accessToken'); localStorage.removeItem('refreshToken'); } catch(e) {}
  setAuthToken(null);
};

export const refreshToken = async () => {
  const refresh = localStorage.getItem('refreshToken');
  if (!refresh) throw new Error('no refresh token');
  const resp = await axios.post(REFRESH_URL, { RefreshToken: refresh, AccessToken: localStorage.getItem('accessToken') });
  const data = resp.data;
  try { localStorage.setItem('accessToken', data.AccessToken); localStorage.setItem('refreshToken', data.RefreshToken); } catch (e) {}
  setAuthToken(data.AccessToken);
  return data;
};
