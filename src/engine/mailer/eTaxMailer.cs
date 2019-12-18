using System;
using System.ServiceProcess;

namespace OpenTax.Engine.Mailer
{
    public partial class eTaxMailer : ServiceBase
    {
        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        public eTaxMailer()
        {
            InitializeComponent();
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        private OpenTax.Engine.Mailer.Host m_mailHoster = null;
        private OpenTax.Engine.Mailer.Host MailHoster
        {
            get
            {
                if (m_mailHoster == null)
                    m_mailHoster = new OpenTax.Engine.Mailer.Host();

                return m_mailHoster;
            }
        }

        private OpenTax.Engine.Mailer.Worker m_mailWorker = null;
        private OpenTax.Engine.Mailer.Worker MailWorker
        {
            get
            {
                if (m_mailWorker == null)
                    m_mailWorker = new OpenTax.Engine.Mailer.Worker();

                return m_mailWorker;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        protected override void OnStart(string[] args)
        {
            ELogger.SNG.WriteLog("server service start...");

            MailHoster.Start();                // Starting WCF server.
            MailWorker.Start();                // Running service to send mail automatically.

            base.OnStart(args);
        }

        protected override void OnStop()
        {
            base.OnStop();

            MailWorker.Stop();
            MailHoster.Stop();

            ELogger.SNG.WriteLog("server service stop...");
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
    }
}