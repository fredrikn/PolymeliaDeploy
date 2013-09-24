using System.Activities;
using System.ComponentModel;

namespace PolymeliaDeploy.Activities
{
    using System.Messaging;
    using System.Security.Principal;

    [DisplayName("Create MSMQ Queue"), Category("Actions")]
    public class CreateMsmqQueue : PolymeliaNativiveActivity
    {
        [RequiredArgument]
        [DescriptionAttribute("The name of the Queue to create")]
        public InArgument<string> QueueName { get; set; }

        [RequiredArgument]
        [DescriptionAttribute("The SID of the user that should have Read and Write permission to the Queue")]
        public InArgument<string> UserSID { get; set; }

        [RequiredArgument]
        [DescriptionAttribute("Transactional queue")]
        public InArgument<bool> TransactionalQueue { get; set; }


        protected override void Execute(NativeActivityContext context)
        {
            ReportInfo(string.Format("Start creating queue '{0}'", context.GetValue(QueueName)), context);

            if (!MessageQueue.Exists(context.GetValue(QueueName)))
            {
                CreateQueue(context);

                ReportInfo(string.Format("Succeeded to creat queue '{0}'", context.GetValue(QueueName)), context);
            }
            else
            {
                ReportInfo(string.Format("Queue '{0}' already exists", context.GetValue(QueueName)), context);
            }
        }

        private void CreateQueue(NativeActivityContext context)
        {
            var queue = MessageQueue.Create(context.GetValue(QueueName), context.GetValue(TransactionalQueue));

            var tr = new Trustee(GetNameBySid(context.GetValue(UserSID)));

            queue.SetPermissions(
                new AccessControlList
                    {
                        new AccessControlEntry(
                            tr,
                            GenericAccessRights.Read,
                            StandardAccessRights.Read,
                            AccessControlEntryType.Allow),
                        new AccessControlEntry(
                            tr,
                            GenericAccessRights.Write,
                            StandardAccessRights.Write,
                            AccessControlEntryType.Allow)
                    });
        }

        private static string GetNameBySid(string sid)
        {
            return new SecurityIdentifier(sid).Translate(typeof(NTAccount)).ToString();
        }
    }
}