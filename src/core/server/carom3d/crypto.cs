using System;

namespace core
{
    public class CryptoContext
    {
        private byte m_cryptoByte;

        public CryptoContext()
        {
            m_cryptoByte = 0x02;
        }

        public void Crypt(byte[] data, uint len)
        {
            for (int i = 0; i < len; ++i)
            {
                data[i] ^= m_cryptoByte;
            }
        }

        public void Update()
        {
            m_cryptoByte = (byte)(m_cryptoByte * m_cryptoByte + m_cryptoByte * 2 + m_cryptoByte / 3 + 4);
        }
    }
}
