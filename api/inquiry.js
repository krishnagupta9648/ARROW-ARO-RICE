// Vercel Serverless Function for ARROW ARO RICE Inquiry API
module.exports = async (req, res) => {
  res.setHeader('Access-Control-Allow-Credentials', true);
  res.setHeader('Access-Control-Allow-Origin', '*');
  res.setHeader('Access-Control-Allow-Methods', 'GET,OPTIONS,PATCH,DELETE,POST,PUT');
  res.setHeader(
    'Access-Control-Allow-Headers',
    'X-CSRF-Token, X-Requested-With, Accept, Accept-Version, Content-Length, Content-MD5, Content-Type, Date, X-Api-Version'
  );

  if (req.method === 'OPTIONS') {
    res.status(200).end();
    return;
  }

  if (req.method === 'POST') {
    const inquiry = req.body || {};
    const timestamp = new Date().toISOString();
    console.log('[Inquiry Received]', timestamp, inquiry);

    return res.status(200).json({
      success: true,
      message: 'Inquiry received successfully. Our export sales team will reach out within 4 business hours.',
      inquiryNumber: 'INQ-' + new Date().getFullYear() + '-' + Math.floor(1000 + Math.random() * 9000),
      timestamp: timestamp,
      received: inquiry
    });
  }

  return res.status(200).json({
    status: 'online',
    service: 'ARROW ARO RICE International Trade Desk API',
    company: 'ARROW ARO RICE PRIVATE LIMITED',
    hq: 'Karnal, Haryana & Mumbai, India'
  });
};
