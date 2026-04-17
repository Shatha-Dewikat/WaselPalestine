import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  vus: 5,
  duration: '1m',
  thresholds: {
    http_req_duration: ['p(95)<2000'],
    http_req_failed: ['rate<0.2'],
  },
};

const token = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJmM2RiMThiYy0wMWNkLTQzZWMtOWNmMy0zNGNjMDFjN2JjODAiLCJVc2VySWQiOiJmM2RiMThiYy0wMWNkLTQzZWMtOWNmMy0zNGNjMDFjN2JjODAiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoibG9hZHRlc3QxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvZW1haWxhZGRyZXNzIjoibG9hZHRlc3QxQGdtYWlsLmNvbSIsImp0aSI6ImE1MjM2ZDI5LTc1M2MtNGNmYS1hMDk1LWFhNGE5Y2ViNWZmZCIsImlzQWN0aXZlIjoidHJ1ZSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlVzZXIiLCJleHAiOjE3NzY0MTYwMTAsImlzcyI6Ildhc2VsUGFsZXN0aW5lIiwiYXVkIjoiV2FzZWxQYWxlc3RpbmUifQ.eDwEEsC_3LWwdD28RjqsqqDFro9oXapj06XTlV-jXjU';

export default function () {
  const payload = JSON.stringify({
    latitude: 31.501,
    longitude: 34.466,
    description: `k6 write test ${__VU}-${__ITER}`,
    categoryId: 1,
    userId: 'f3db18bc-01cd-43ec-9cf3-34cc01c7bc80',
    city: 'Nablus',
    areaName: 'Downtown',
  });

  const res = http.post(
    'http://localhost:5034/api/v1/Reports/submit',
    payload,
    {
      headers: {
        Authorization: `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
    }
  );

  check(res, {
    'status ok': (r) => r.status === 200 || r.status === 201,
  });

  sleep(1);
}