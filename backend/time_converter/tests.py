from django.test import TestCase


class MJDAPITest(TestCase):
	def test_api_mjd_returns_mjd(self):
		resp = self.client.get('/api/mjd')
		self.assertEqual(resp.status_code, 200)
		data = resp.json()
		self.assertIn('mjd', data)
		# mjd should be returned as a string and contain only digits
		self.assertIsInstance(data['mjd'], str)
		self.assertTrue(data['mjd'].isdigit())
