import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  vus: 20,
  duration: '1m',
  thresholds: {
    http_req_failed: ['rate<0.05'],
    http_req_duration: ['p(95)<1500'],
  },
};

const token = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJmM2RiMThiYy0wMWNkLTQzZWMtOWNmMy0zNGNjMDFjN2JjODAiLCJVc2VySWQiOiJmM2RiMThiYy0wMWNkLTQzZWMtOWNmMy0zNGNjMDFjN2JjODAiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoibG9hZHRlc3QxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvZW1haWxhZGRyZXNzIjoibG9hZHRlc3QxQGdtYWlsLmNvbSIsImp0aSI6IjY3YjA1MDE4LTcwNDAtNDljMi05NWYzLWFmYThkODk2ZDI1OSIsImlzQWN0aXZlIjoidHJ1ZSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlVzZXIiLCJleHAiOjE3NzYzODE3MzQsImlzcyI6Ildhc2VsUGFsZXN0aW5lIiwiYXVkIjoiV2FzZWxQYWxlc3RpbmUifQ.2zNh-Cb2ATAS3L8BytrUGf_w55mvdYFx9LshJCwp91o';

export default function () {
  const res = http.get(
    'http://localhost:5034/api/v1/Incidents/paged?pageNumber=1&pageSize=10&lang=en',
    {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    }
  );

  if (res.status !== 200) {
    console.log(`status=${res.status} body=${res.body}`);
  }

  check(res, {
    'status is 200': (r) => r.status === 200,
    'success true': (r) => {
      try {
        return r.json().success === true;
      } catch {
        return false;
      }
    },
  });

  sleep(1);
}