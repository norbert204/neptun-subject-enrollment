import axios from 'axios';

const ADMIN_BASE = '/api/Admin';

export const createUser = (user) => axios.post(`${ADMIN_BASE}/user`, user);

export const startEnrollmentPeriod = () => axios.post(`${ADMIN_BASE}/start-enrollment-period`);
