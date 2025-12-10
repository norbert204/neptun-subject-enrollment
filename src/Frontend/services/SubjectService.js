import axios from 'axios';

// Use relative path so requests go through frontend nginx proxy (/api/* -> gateway)
const REST_API_BASE_URL = '/api/Subject';

export const getEligibleCoursesForStudent = (studentId) =>
  axios.get(`${REST_API_BASE_URL}/student/${studentId}/eligible-courses`);

export const EnrollInCourse = (studentId, courseId) =>
  axios.post(`${REST_API_BASE_URL}/enroll-to-course`, {
    StudentId: studentId,
    CourseId: courseId,
  });

export const getEnrolledCourses = (studentId) =>
  axios.get(`${REST_API_BASE_URL}/student/${studentId}/enrolled`);